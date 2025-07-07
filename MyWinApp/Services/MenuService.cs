using RestaurantOrderManagement.Data;
using RestaurantOrderManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;

namespace RestaurantOrderManagement.Services
{
    public class MenuService
    {
        private readonly RestaurantDbContext _context;

        public MenuService(RestaurantDbContext context)
        {
            _context = context;
        }

        public List<MenuItem> GetAllMenuItems()
        {
            return _context.MenuItems
                .Where(item => item.IsAvailable)
                .OrderBy(item => item.Category)
                .ThenBy(item => item.Name)
                .ToList();
        }

        public List<MenuItem> GetMenuItemsByCategory(Category category)
        {
            return _context.MenuItems
                .Where(item => item.Category == category && item.IsAvailable)
                .OrderBy(item => item.Name)
                .ToList();
        }

        public MenuItem? GetMenuItemById(int id)
        {
            return _context.MenuItems.FirstOrDefault(item => item.Id == id);
        }

        public MenuItem AddMenuItem(MenuItem menuItem)
        {
            var maxId = _context.MenuItems.Any() ? _context.MenuItems.Max(item => item.Id) : 0;
            menuItem.Id = maxId + 1;
            _context.MenuItems.Add(menuItem);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to add menu item: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            return menuItem;
        }

        public bool UpdateMenuItem(MenuItem menuItem)
        {
            var existingItem = _context.MenuItems.FirstOrDefault(item => item.Id == menuItem.Id);
            if (existingItem != null)
            {
                existingItem.Name = menuItem.Name;
                existingItem.Description = menuItem.Description;
                existingItem.Price = menuItem.Price;
                existingItem.Category = menuItem.Category;
                existingItem.IsAvailable = menuItem.IsAvailable;
                existingItem.ImagePath = menuItem.ImagePath;
                existingItem.PreparationTimeMinutes = menuItem.PreparationTimeMinutes;
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to update menu item: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
                return true;
            }
            return false;
        }

        public bool RemoveMenuItem(int id)
        {
            var item = _context.MenuItems.FirstOrDefault(item => item.Id == id);
            if (item != null)
            {
                _context.MenuItems.Remove(item);
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to delete menu item: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
                return true;
            }
            return false;
        }

        public bool SetItemAvailability(int id, bool isAvailable)
        {
            var item = _context.MenuItems.FirstOrDefault(item => item.Id == id);
            if (item != null)
            {
                item.IsAvailable = isAvailable;
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to update item availability: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
                return true;
            }
            return false;
        }

        public bool UpdateItemPrice(int id, decimal newPrice)
        {
            var item = _context.MenuItems.FirstOrDefault(item => item.Id == id);
            if (item != null)
            {
                item.Price = newPrice;
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to update item price: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
                return true;
            }
            return false;
        }

        public bool UpdateItemName(int id, string newName)
        {
            var item = _context.MenuItems.FirstOrDefault(item => item.Id == id);
            if (item != null && !string.IsNullOrWhiteSpace(newName))
            {
                item.Name = newName.Trim();
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to update item name: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
                return true;
            }
            return false;
        }

        public List<MenuItem> GetAllMenuItemsForManagement()
        {
            return _context.MenuItems
                .OrderBy(item => item.Category)
                .ThenBy(item => item.Name)
                .ToList();
        }

        public bool ItemExists(string name, int? excludeId = null)
        {
            return _context.MenuItems
                .Any(item => item.Name.ToLower() == name.ToLower() && 
                            (!excludeId.HasValue || item.Id != excludeId.Value));
        }
    }
} 