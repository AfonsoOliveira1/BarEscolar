using BarEscolar.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BarEscolar.Services
{
    public class JsonOrderStore
    {
        private readonly string _ordersFile = "Data/orders.json";
        private readonly string _orderItemsFile = "Data/orderItems.json";

        private List<Order> _orders;
        private List<OrderItem> _orderItems;

        public JsonOrderStore()
        {
            _orders = LoadOrders();
            _orderItems = LoadOrderItems();
        }

        // ---------------- LOAD / SAVE ----------------

        private List<Order> LoadOrders()
        {
            if (!File.Exists(_ordersFile)) return new List<Order>();
            var json = File.ReadAllText(_ordersFile);
            return JsonSerializer.Deserialize<List<Order>>(json) ?? new List<Order>();
        }

        private List<OrderItem> LoadOrderItems()
        {
            if (!File.Exists(_orderItemsFile)) return new List<OrderItem>();
            var json = File.ReadAllText(_orderItemsFile);
            return JsonSerializer.Deserialize<List<OrderItem>>(json) ?? new List<OrderItem>();
        }

        private void SaveOrders() =>
            File.WriteAllText(_ordersFile, JsonSerializer.Serialize(_orders, new JsonSerializerOptions { WriteIndented = true }));

        private void SaveOrderItems() =>
            File.WriteAllText(_orderItemsFile, JsonSerializer.Serialize(_orderItems, new JsonSerializerOptions { WriteIndented = true }));

        // ---------------- ORDERS ----------------

        public List<Order> GetOrders() => _orders;

        public List<Order> GetOrdersByUser(string userId)
        {
            var orders = _orders.Where(o => o.Userid == userId).ToList();

            foreach (var order in orders)
                order.OrderItems = _orderItems.Where(oi => oi.Orderid == order.Id).ToList();

            return orders;
        }

        public Order GetOrderById(int orderId) =>
            _orders.FirstOrDefault(o => o.Id == orderId);

        public void AddOrder(Order order)
        {
            _orders.Add(order);
            SaveOrders();
        }

        public void RemoveOrder(int orderId)
        {
            var order = GetOrderById(orderId);
            if (order != null)
            {
                _orderItems.RemoveAll(oi => oi.Orderid == orderId);
                _orders.Remove(order);

                SaveOrders();
                SaveOrderItems();
            }
        }

        public int GetNextOrderId() =>
            _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1;

        // ---------------- ORDER ITEMS ----------------

        public List<OrderItem> GetOrderItems() => _orderItems;

        public List<OrderItem> GetOrderItemsByOrder(int orderId) =>
            _orderItems.Where(oi => oi.Orderid == orderId).ToList();

        public OrderItem GetOrderItemById(int id) =>
            _orderItems.FirstOrDefault(oi => oi.Id == id);

        public void AddOrderItem(OrderItem item)
        {
            _orderItems.Add(item);
            SaveOrderItems();
        }

        public void RemoveOrderItem(int orderItemId)
        {
            var item = GetOrderItemById(orderItemId);
            if (item != null)
            {
                _orderItems.Remove(item);

                // Remove order se não tiver items
                var order = GetOrderById(item.Orderid);
                if (order != null && !_orderItems.Any(oi => oi.Orderid == order.Id))
                    _orders.Remove(order);

                SaveOrders();
                SaveOrderItems();
            }
        }

        public int GetNextOrderItemId() =>
            _orderItems.Any() ? _orderItems.Max(oi => oi.Id) + 1 : 1;

        // ---------------- MARCAR MENU ----------------

        public void MarkMenu(string userId, int menuId)
        {
            var today = DateTime.Today;

            var order = _orders.FirstOrDefault(o =>
                o.Userid == userId &&
                o.Createdat.Date == today
            );

            if (order == null)
            {
                order = new Order
                {
                    Id = GetNextOrderId(),
                    Userid = userId,
                    Createdat = DateTime.Now,
                    Total = 0,
                    State = true
                };

                _orders.Add(order);
            }

            var item = new OrderItem
            {
                Id = GetNextOrderItemId(),
                Orderid = order.Id,
                Productid = menuId,
                IsMenu = true, // ← AQUI ESTÁ O FIX!
                Quantity = 1,
                Unitprice = 0,
                Subtotal = 0
            };

            _orderItems.Add(item);

            SaveOrders();
            SaveOrderItems();
        }

        // ---------------- HELPERS ----------------

        public Dictionary<int, int> GetRemainingSeats()
        {
            return _orderItems
                .Where(o => o.IsMenu) // só menus contam
                .GroupBy(oi => oi.Productid)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        public void BuyProduct(string userId, int productId, double price)
        {
            var order = _orders.FirstOrDefault(o =>
                o.Userid == userId &&
                o.Createdat.Date == DateTime.Today
            );

            if (order == null)
            {
                order = new Order
                {
                    Id = GetNextOrderId(),
                    Userid = userId,
                    Createdat = DateTime.Now,
                    Total = 0,
                    State = true
                };

                _orders.Add(order);
            }

            var item = new OrderItem
            {
                Id = GetNextOrderItemId(),
                Orderid = order.Id,
                Productid = productId,
                Quantity = 1,
                Unitprice = price,
                Subtotal = price,
                IsMenu = false,          // ← AGORA É PRODUTO, NÃO MENU
                CreatedAt = DateTime.Now
            };

            _orderItems.Add(item);

            order.Total += price;

            SaveOrders();
            SaveOrderItems();
        }

    }
}
