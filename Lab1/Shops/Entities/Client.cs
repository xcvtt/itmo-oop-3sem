﻿using Shops.Exceptions;

namespace Shops.Entities;

public class Client
{
    private readonly int _id;

    public Client(int id, string name, decimal cash)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new StringNullOrWhiteSpaceException($"{nameof(name)} was null or whitespace.");
        }

        if (cash < 0)
        {
            throw ClientException.InvalidCash(cash);
        }

        _id = id;
        Name = name;
        Cash = cash;
    }

    public decimal Cash { get; private set; }
    public string Name { get; }

    public void ProcessTransaction(decimal requestedCash)
    {
        if (Cash < requestedCash)
        {
            throw ClientException.NotEnoughMoney(Cash, requestedCash);
        }

        Cash -= requestedCash;
    }
}