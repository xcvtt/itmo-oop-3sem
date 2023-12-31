﻿using Banks.Accounts;
using Banks.Exceptions;

namespace Banks.Commands;

public class TransferTransaction : ITransaction
{
    private readonly IAccount _accountFrom;
    private readonly IAccount _accountTo;
    private readonly decimal _transferAmount;
    private readonly IAccountLimits _accountLimitsFrom;
    private readonly IAccountLimits _accountLimitsTo;
    private readonly decimal _beforeBalanceFrom;
    private readonly decimal _beforeBalanceTo;
    private bool _hasRun;

    public TransferTransaction(IAccount accountFrom, IAccount accountTo, decimal transferAmount)
    {
        ArgumentNullException.ThrowIfNull(accountFrom);
        ArgumentNullException.ThrowIfNull(accountTo);
        if (transferAmount < 0)
        {
            throw new BankException("Transfer amount can't be negative");
        }

        _accountFrom = accountFrom;
        _accountTo = accountTo;
        _transferAmount = transferAmount;
        _accountLimitsFrom = accountFrom.AccountLimits;
        _accountLimitsTo = accountTo.AccountLimits;
        _beforeBalanceFrom = accountFrom.Balance;
        _beforeBalanceTo = accountTo.Balance;
        _hasRun = false;
        TransactionId = ITransaction.transactionCounter++;
    }

    public int TransactionId { get; }

    public void Run()
    {
        if (_hasRun)
        {
            throw new BankException("Transaction was already executed");
        }

        if (_accountLimitsFrom.WithdrawTransferLimit < _transferAmount)
        {
            throw new BankException("Transfer amount exceeds limit");
        }

        decimal newBalanceFrom = _beforeBalanceFrom - _transferAmount;
        if (_accountLimitsFrom.TransactionCommission > 0 && _beforeBalanceFrom < 0)
        {
            newBalanceFrom -= _accountLimitsFrom.TransactionCommission;
        }

        if (!_accountLimitsFrom.CanGoNegative && newBalanceFrom < 0)
        {
            throw new BankException("Balance can't go negative for this account");
        }

        decimal newBalanceTo = _beforeBalanceTo + _transferAmount;

        _accountFrom.Balance = newBalanceFrom;
        _accountTo.Balance = newBalanceTo;
        _hasRun = true;
    }

    public void Revert()
    {
        if (!_hasRun)
        {
            throw new BankException("Transaction wasn't executed or was already reverted");
        }

        decimal currentBalanceFrom = _accountFrom.Balance;
        decimal currentBalanceTo = _accountTo.Balance;
        decimal newBalanceFrom = currentBalanceFrom + _transferAmount;
        decimal newBalanceTo = currentBalanceTo - _transferAmount;
        if (_accountLimitsFrom.TransactionCommission > 0 && _beforeBalanceFrom < 0)
        {
            newBalanceFrom += _accountLimitsFrom.TransactionCommission;
        }

        _accountFrom.Balance = newBalanceFrom;
        _accountTo.Balance = newBalanceTo;
        _hasRun = false;
    }

    public override string ToString()
    {
        return $"{TransactionId}. Type: Transfer, Balance before: {_beforeBalanceFrom}, " +
               $"Transfer amount: {_transferAmount}, Acc ID: {_accountFrom.AccountId}";
    }
}