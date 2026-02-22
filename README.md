# Zadanie rekrutacyjne

Projekt demonstracyjny składający się z dwóch mikroserwisów komunikujących się asynchronicznie przez emulator Azure Service Bus. System obsługuje zasilanie i konwersję rzadszych walut, definiowanych wyłącznie przez zbiór walut Tabeli B NBP.

## Architektura

Rozwiązanie wykorzystuje konteneryzację (Docker) do uruchomienia wszystkich wymaganych zależności.
Głównymi filarami są:

1. **RatesService**: Serwis pobiera kursy walut z API NPB.
2. **WalletService**: Serwis zarządzający portfelami.

## Jak uruchomić projekt

### Wymagania

- **Docker**

Aby uruchomić projekt należy wpisać w terminalu komendę:

```bash
docker compose up --build -d
```

Domyślne adresy serwisów:

- `http://localhost:5100/` - RatesService
- `http://localhost:5200/` - WalletService

## Przykładowe operacje

Stworzenie nowego pustego portfela:

```powershell
Invoke-RestMethod -Uri "http://localhost:5200/api/wallets" -Method Post -Headers @{"Content-Type"="application/json"} -Body '{"name": "Mój pierwszy portfel"}'
```

Wpłata waluty

```powershell
Invoke-RestMethod -Uri "http://localhost:5200/api/wallets/{id}/deposit" -Method Post -Headers @{"Content-Type"="application/json"} -Body '{"currencyCode": "AFN", "amount": 100}'
```

Konwersja z waluty **AFN** na **AMD**

```powershell
Invoke-RestMethod -Uri "http://localhost:5200/api/wallets/{id}/convert" -Method Post -Headers @{"Content-Type"="application/json"} -Body '{"fromCurrency": "AFN", "toCurrency": "AMD", "amount": 50}'
```

Odczyt stanu wirtualnego portfela

```powershell
Invoke-RestMethod -Uri "http://localhost:5200/api/wallets/{id}" -Method Get
```
