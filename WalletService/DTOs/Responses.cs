namespace WalletService.DTOs;

public record WalletResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    List<BalanceResponse> Balances
);

public record BalanceResponse(string CurrencyCode, decimal Amount);

public record BalanceUpdateResponse(string CurrencyCode, decimal NewBalance);

public record ConvertResponse(
    ConvertCurrencyDetail From,
    ConvertCurrencyDetail To,
    decimal ExchangeRate
);

public record ConvertCurrencyDetail(string Currency, decimal Amount, decimal NewBalance);
