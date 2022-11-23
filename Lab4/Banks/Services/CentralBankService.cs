﻿using Banks.Accounts;
using Banks.Entities;
using Banks.Exceptions;
using Banks.Models;

namespace Banks.Services;

public class CentralBankService
{
    private readonly ICollection<Bank> _banks;
    private readonly ICollection<Client> _clients;
    private readonly ICollection<IAccount> _accounts;

    public CentralBankService()
    {
        _banks = new List<Bank>();
        _clients = new List<Client>();
        _accounts = new List<IAccount>();
    }

    public Bank RegisterNewBank(string bankName, BankConfig bankConfig)
    {
        Bank bank = new Bank(bankName, bankConfig, _banks.Count);
        _banks.Add(bank);
        return bank;
    }

    public Client RegisterNewClient(Bank bank, ClientName clientName, ClientAddress clientAddress = null, ClientPassportId clientPassportId = null)
    {
        var targetBank = _banks.FirstOrDefault(b => b.Equals(bank));
        if (targetBank is null)
        {
            throw new BankException("Bank isn't registered in the central bank");
        }

        Client client = new ClientBuilder()
            .SetClientName(clientName)
            .SetClientAddress(clientAddress)
            .SetClientPassportId(clientPassportId)
            .SetClientId(_clients.Count)
            .GetClient();

        bank.AddClient(client);
        _clients.Add(client);
        return client;
    }

    public IAccount RegisterNewAccount(Bank bank, Client client, AccountType accountType, decimal depositAmount = 0)
    {
        var targetBank = _banks.FirstOrDefault(b => b.Equals(bank));
        if (targetBank is null)
        {
            throw new BankException("Bank isn't registered in the central bank");
        }

        var targetClient = _clients.FirstOrDefault(c => c.Equals(client));
        if (targetClient is null)
        {
            throw new BankException("Client isn't registered in the central bank");
        }

        IAccount account = accountType switch
        {
            AccountType.Credit => new CreditAccount(client, bank, _accounts.Count, depositAmount),
            AccountType.Debit => new DebitAccount(client, bank, _accounts.Count, depositAmount),
            AccountType.Deposit => new DepositAccount(client, bank, _accounts.Count, depositAmount),
            _ => throw new BankException("Account type not implemented"),
        };

        bank.AddAccount(account, client);
        _accounts.Add(account);
        return account;
    }

    public void TransferFromAccountTo(int accountIdFrom, int accountIdTo, decimal transferAmount)
    {
        var accountFrom = _accounts.FirstOrDefault(x => x.AccountId.Equals(accountIdFrom));
        var accountTo = _accounts.FirstOrDefault(x => x.AccountId.Equals(accountIdTo));
    }
}