using RestaurantOrderManagement.Data;
using RestaurantOrderManagement.Models;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;

namespace RestaurantOrderManagement.Services
{
    public class PaymentService
    {
        private readonly RestaurantDbContext _context;

        public PaymentService(RestaurantDbContext context)
        {
            _context = context;
        }

        public Payment? GetPaymentByOrderId(int orderId)
        {
            return _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o!.Items)
                .FirstOrDefault(p => p.OrderId == orderId);
        }

        public Payment ProcessCashPayment(int orderId, decimal amountPaid, string cashierName, string notes = "")
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                throw new ArgumentException("Order not found");

            // Prevent duplicate payments
            if (_context.Payments.Any(p => p.OrderId == orderId))
                throw new InvalidOperationException("A payment has already been processed for this order.");

            var totalAmount = order.Items.Sum(item => item.Price * item.Quantity);
            
            if (amountPaid < totalAmount)
                throw new ArgumentException("Amount paid is less than total amount");

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = totalAmount,
                AmountPaid = amountPaid,
                Change = amountPaid - totalAmount,
                Status = PaymentStatus.Completed,
                CashierName = cashierName,
                Notes = notes
            };

            try
            {
                _context.Payments.Add(payment);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to process payment: {ex.InnerException?.Message ?? ex.Message}", ex);
            }

            return payment;
        }

        public List<Payment> GetPaymentsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentTime >= startDate && p.PaymentTime <= endDate)
                .OrderByDescending(p => p.PaymentTime)
                .ToList();
        }

        public decimal GetTotalRevenueByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && 
                           p.PaymentTime >= startDate && 
                           p.PaymentTime <= endDate)
                .Sum(p => p.Amount);
        }

        public void PrintReceipt(Payment payment)
        {
            var printDocument = new PrintDocument();
            printDocument.PrintPage += (sender, e) => PrintReceiptPage(sender, e, payment);
            
            try
            {
                printDocument.Print();
            }
            catch (Exception ex)
            {
                // If printing fails, show the receipt in a preview window
                ShowReceiptPreview(payment);
            }
        }

        private void PrintReceiptPage(object sender, PrintPageEventArgs e, Payment payment)
        {
            var graphics = e.Graphics;
            var font = new Font("Courier New", 10);
            var boldFont = new Font("Courier New", 12, FontStyle.Bold);
            var titleFont = new Font("Arial", 14, FontStyle.Bold);
            
            var yPosition = 10;
            var leftMargin = 10;
            var lineHeight = 20;

            // Restaurant header
            graphics.DrawString("RESTAURANT ORDER MANAGEMENT", titleFont, Brushes.Black, leftMargin, yPosition);
            yPosition += lineHeight + 5;
            graphics.DrawString("123 Main Street, City, State", font, Brushes.Black, leftMargin, yPosition);
            yPosition += lineHeight;
            graphics.DrawString("Phone: (555) 123-4567", font, Brushes.Black, leftMargin, yPosition);
            yPosition += lineHeight + 10;

            // Receipt details
            graphics.DrawString($"Receipt #: {payment.ReceiptNumber}", boldFont, Brushes.Black, leftMargin, yPosition);
            yPosition += lineHeight;
            graphics.DrawString($"Date: {payment.PaymentTime:MM/dd/yyyy HH:mm}", font, Brushes.Black, leftMargin, yPosition);
            yPosition += lineHeight;
            graphics.DrawString($"Cashier: {payment.CashierName}", font, Brushes.Black, leftMargin, yPosition);
            yPosition += lineHeight + 10;

            // Order details
            graphics.DrawString("Order Details:", boldFont, Brushes.Black, leftMargin, yPosition);
            yPosition += lineHeight;

            if (payment.Order != null)
            {
                graphics.DrawString($"Order #: {payment.Order.OrderId}", font, Brushes.Black, leftMargin, yPosition);
                yPosition += lineHeight;
                graphics.DrawString($"Customer: {payment.Order.CustomerName}", font, Brushes.Black, leftMargin, yPosition);
                yPosition += lineHeight;
                
                if (!string.IsNullOrEmpty(payment.Order.TableNumber))
                {
                    graphics.DrawString($"Table: {payment.Order.TableNumber}", font, Brushes.Black, leftMargin, yPosition);
                    yPosition += lineHeight;
                }
                
                yPosition += 5;

                // Items
                graphics.DrawString("Items:", boldFont, Brushes.Black, leftMargin, yPosition);
                yPosition += lineHeight;

                foreach (var item in payment.Order.Items)
                {
                    var itemLine = $"{item.Quantity}x {item.Name}";
                    graphics.DrawString(itemLine, font, Brushes.Black, leftMargin, yPosition);
                    
                    var priceLine = $"${item.Price * item.Quantity:F2}";
                    var priceWidth = graphics.MeasureString(priceLine, font).Width;
                    graphics.DrawString(priceLine, font, Brushes.Black, e.PageBounds.Width - leftMargin - priceWidth, yPosition);
                    
                    yPosition += lineHeight;
                }
            }

            yPosition += 10;

            // Totals
            graphics.DrawString("----------------------------------------", font, Brushes.Black, leftMargin, yPosition);
            yPosition += lineHeight;
            
            var totalLine = $"Total: ${payment.Amount:F2}";
            var totalWidth = graphics.MeasureString(totalLine, boldFont).Width;
            graphics.DrawString(totalLine, boldFont, Brushes.Black, e.PageBounds.Width - leftMargin - totalWidth, yPosition);
            yPosition += lineHeight;
            
            var paidLine = $"Paid: ${payment.AmountPaid:F2}";
            var paidWidth = graphics.MeasureString(paidLine, font).Width;
            graphics.DrawString(paidLine, font, Brushes.Black, e.PageBounds.Width - leftMargin - paidWidth, yPosition);
            yPosition += lineHeight;
            
            var changeLine = $"Change: ${payment.Change:F2}";
            var changeWidth = graphics.MeasureString(changeLine, font).Width;
            graphics.DrawString(changeLine, font, Brushes.Black, e.PageBounds.Width - leftMargin - changeWidth, yPosition);
            yPosition += lineHeight + 10;

            // QR Code
            var qrCodeData = GenerateQRCodeData(payment);
            var qrCode = GenerateQRCode(qrCodeData);
            
            if (qrCode != null)
            {
                var qrSize = 80;
                var qrX = (e.PageBounds.Width - qrSize) / 2;
                graphics.DrawImage(qrCode, qrX, yPosition, qrSize, qrSize);
                yPosition += qrSize + 10;
            }

            // Footer
            graphics.DrawString("Thank you for your business!", boldFont, Brushes.Black, leftMargin, yPosition);
            yPosition += lineHeight;
            graphics.DrawString("Please come again!", font, Brushes.Black, leftMargin, yPosition);
        }

        private string GenerateQRCodeData(Payment payment)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Receipt: {payment.ReceiptNumber}");
            sb.AppendLine($"Date: {payment.PaymentTime:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Amount: ${payment.Amount:F2}");
            sb.AppendLine($"Order: #{payment.OrderId}");
            sb.AppendLine($"Restaurant: Restaurant Order Management");
            return sb.ToString();
        }

        private Bitmap? GenerateQRCode(string data)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrCodeData);
                return qrCode.GetGraphic(20);
            }
            catch
            {
                return null;
            }
        }

        private void ShowReceiptPreview(Payment payment)
        {
            var receiptForm = new Form
            {
                Text = $"Receipt Preview - {payment.ReceiptNumber}",
                Size = new Size(400, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var receiptText = GenerateReceiptText(payment);
            
            var textBox = new TextBox
            {
                Text = receiptText,
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Courier New", 10),
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical
            };

            var printButton = new Button
            {
                Text = "Print Receipt",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White
            };
            printButton.Click += (s, e) => PrintReceipt(payment);

            panel.Controls.Add(textBox);
            panel.Controls.Add(printButton);
            receiptForm.Controls.Add(panel);
            receiptForm.Show();
        }

        private string GenerateReceiptText(Payment payment)
        {
            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("        RESTAURANT ORDER MANAGEMENT");
            sb.AppendLine("========================================");
            sb.AppendLine("123 Main Street, City, State");
            sb.AppendLine("Phone: (555) 123-4567");
            sb.AppendLine();
            sb.AppendLine($"Receipt #: {payment.ReceiptNumber}");
            sb.AppendLine($"Date: {payment.PaymentTime:MM/dd/yyyy HH:mm}");
            sb.AppendLine($"Cashier: {payment.CashierName}");
            sb.AppendLine();
            sb.AppendLine("Order Details:");
            
            if (payment.Order != null)
            {
                sb.AppendLine($"Order #: {payment.Order.OrderId}");
                sb.AppendLine($"Customer: {payment.Order.CustomerName}");
                
                if (!string.IsNullOrEmpty(payment.Order.TableNumber))
                    sb.AppendLine($"Table: {payment.Order.TableNumber}");
                
                sb.AppendLine();
                sb.AppendLine("Items:");
                
                foreach (var item in payment.Order.Items)
                {
                    sb.AppendLine($"{item.Quantity}x {item.Name,-20} ${item.Price * item.Quantity,8:F2}");
                }
            }
            
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Total:                              ${payment.Amount,8:F2}");
            sb.AppendLine($"Paid:                               ${payment.AmountPaid,8:F2}");
            sb.AppendLine($"Change:                             ${payment.Change,8:F2}");
            sb.AppendLine();
            sb.AppendLine("Thank you for your business!");
            sb.AppendLine("Please come again!");
            sb.AppendLine("========================================");
            
            return sb.ToString();
        }
    }
} 