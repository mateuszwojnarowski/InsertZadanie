namespace WalletService.DTOs;

// Wallet
public record CreateWalletRequest(string Name);

// Transactions
public record DepositRequest(string CurrencyCode, decimal Amount);
public record WithdrawRequest(string CurrencyCode, decimal Amount);
public record ConvertRequest(string FromCurrency, string ToCurrency, decimal Amount);
