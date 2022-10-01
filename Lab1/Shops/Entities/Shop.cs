﻿using Shops.Exceptions;

namespace Shops.Entities;

public class Shop : IEquatable<Shop>
{
    private readonly int _id;
    private string _name;
    private string _address;
    private decimal _profit;
    private List<ProductInfo> _products;

    public Shop(int id, string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
        {
            throw new StringNullOrWhiteSpaceException($"{nameof(name)} or {nameof(address)} was null or whitespace.");
        }

        _id = id;
        _name = name;
        _address = address;
        _profit = 0;
        _products = new List<ProductInfo>();
    }

    public void AddProducts(IEnumerable<ProductInfo> products) // params with tuple (product, quantity, price)?
    {
        _products.AddRange(products.Except(_products));
    }

    public void AddProducts(params (Product product, int quantity, decimal price)[] products)
    {
        _products.AddRange(products.Select(
            x => new ProductInfo(x.product, x.quantity, x.price)).Except(_products));
    }

    public void ChangeProductPrice(Product product, decimal newPrice)
    {
        var foundProduct = _products.FirstOrDefault(x => x.Product.Equals(product));
        if (foundProduct != null)
        {
            foundProduct.Price = newPrice;
        }
    }

    public decimal CheckIfAllExistsAndEnoughQuantity(params (Product product, int quantity)[] buyList)
    {
        decimal fullPrice = 0;
        foreach (var pair in buyList)
        {
            var product = _products.FirstOrDefault(t => t.Product.Equals(pair.product));
            if (product == null)
            {
                return -1;
            }

            if (product.Quantity < pair.quantity)
            {
                return 0;
            }

            fullPrice += product.Price * pair.quantity;
        }

        return fullPrice;
    }

    public void SellProductToClient(Client client, params (Product product, int quantity)[] buyList)
    {
        decimal fullProductPrice = CheckIfAllExistsAndEnoughQuantity(buyList);
        switch (fullProductPrice)
        {
            case -1:
                throw new ProductNotFoundException($"Product not found in shop.");
            case 0:
                throw new ProductNotEnoughQuantityException($"Not enough quantity of product.");
        }

        if (client.Cash < fullProductPrice)
        {
            throw new ClientNotEnoughMoneyException($"Client doesn't have enough money. Client: {client.Cash}" +
                                                    $"Needed amount: {fullProductPrice}.");
        }

        foreach (var pair in buyList)
        {
            var product = _products.First(t => t.Product.Equals(pair.product));
            product.Quantity -= pair.quantity;
            if (product.Quantity == 0)
            {
                _products.Remove(product);
            }
        }

        _profit += fullProductPrice;
        client.Cash -= fullProductPrice;
    }

    public override int GetHashCode() => _id.GetHashCode();
    public override bool Equals(object? obj) => Equals(obj as Shop);
    public bool Equals(Shop? other) => other?._id.Equals(_id) ?? false;
}