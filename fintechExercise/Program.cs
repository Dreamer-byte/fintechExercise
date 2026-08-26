public enum TransactionType { Payment, Transfer, Withdrawal }

public class Transaction
{
    public string TransactionId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AccountLimit
{
    public string AccountId { get; set; } = string.Empty;
    public decimal DailyLimit { get; set; }
}

public static class Program
{
    public static void Main()
    {
        var limits = new List<AccountLimit>
        {
            new AccountLimit { AccountId = "ACC01", DailyLimit = 500.00m },
            new AccountLimit { AccountId = "ACC03", DailyLimit = 5000.00m }
            // 'ACC02' no está en la lista, usará el límite por defecto de $500.00m
        };

        // 2. Transacciones de Prueba
        DateTime processingDate = new DateTime(2026, 8, 26);

        var transactions = new List<Transaction>
        {
            // Valida ACC01 ($100)
            new Transaction { TransactionId = "TX01", AccountId = "ACC01", Amount = 100.00m, Type = TransactionType.Payment, Timestamp = new DateTime(2026, 8, 26, 10, 0, 0) },
            
            // Invalida: Fecha errónea
            new Transaction { TransactionId = "TX02", AccountId = "ACC01", Amount = 50.00m, Type = TransactionType.Payment, Timestamp = new DateTime(2026, 8, 25, 10, 0, 0) },
            
            // Invalida: Monto <= 0
            new Transaction { TransactionId = "TX03", AccountId = "ACC01", Amount = -50.00m, Type = TransactionType.Transfer, Timestamp = new DateTime(2026, 8, 26, 11, 0, 0) },
            
            // Valida ACC01 ($400). Total acumulado hoy: $500 (Límite alcanzado)
            new Transaction { TransactionId = "TX04", AccountId = "ACC01", Amount = 400.00m, Type = TransactionType.Payment, Timestamp = new DateTime(2026, 8, 26, 12, 0, 0) },
            
            // Invalida: Excede Límite Diario de ACC01 ($500 + $10 > $500)
            new Transaction { TransactionId = "TX05", AccountId = "ACC01", Amount = 10.00m, Type = TransactionType.Withdrawal, Timestamp = new DateTime(2026, 8, 26, 13, 0, 0) },
            
            // Valida ACC02 ($400). Usa límite por defecto ($500)
            new Transaction { TransactionId = "TX06", AccountId = "ACC02", Amount = 400.00m, Type = TransactionType.Payment, Timestamp = new DateTime(2026, 8, 26, 14, 0, 0) },
            
            // Invalida: Excede Límite por defecto de ACC02 ($400 + $200 > $500)
            new Transaction { TransactionId = "TX07", AccountId = "ACC02", Amount = 200.00m, Type = TransactionType.Payment, Timestamp = new DateTime(2026, 8, 26, 15, 0, 0) },
            
            // Invalida: Alerta Anti-Fraude (> $2000.00m)
            new Transaction { TransactionId = "TX08", AccountId = "ACC03", Amount = 2500.00m, Type = TransactionType.Transfer, Timestamp = new DateTime(2026, 8, 26, 16, 0, 0) }
        };
        List<string> rejectedIds = ProcessAndDetectFraud(transactions, limits, processingDate);

        Console.WriteLine("--- Transacciones Rechazadas / Sospechosas ---");
        foreach (var id in rejectedIds)
        {
            Console.WriteLine($"Rechazada: {id}");
        }
    }

    public static List<string> ProcessAndDetectFraud(
    List<Transaction> transactions,
    List<AccountLimit> limits,
    DateTime processingDate)
    {
        // 1. Guard Clauses (Programación defensiva ante nulls)
        if (transactions == null) return new List<string>();

        var flaggedTransactions = new List<string>();
        var dailySpentByAccount = new Dictionary<string, decimal>();

        // 2. Optimización O(1): Convertir lista a Diccionario antes del bucle
        // Maneja cuentas no encontradas de forma limpia mediante TryGetValue
        var limitsDictionary = limits?.ToDictionary(l => l.AccountId, l => l.DailyLimit)
                               ?? new Dictionary<string, decimal>();

        const decimal DefaultLimit = 500.00m;
        const decimal MaxSingleTransactionLimit = 2000.00m;

        foreach (var transaction in transactions)
        {
            // 3. Reglas 1 y 2: Validaciones básicas de entrada
            if (transaction.Timestamp.Date != processingDate.Date || transaction.Amount <= 0)
            {
                flaggedTransactions.Add(transaction.TransactionId);
                continue; // Clean exit para evitar anidamientos de 'else'
            }

            // 4. Regla 4: Alerta Anti-Fraude
            if (transaction.Amount > MaxSingleTransactionLimit)
            {
                flaggedTransactions.Add(transaction.TransactionId);
                continue;
            }

            // 5. Obtener el límite de la cuenta (O(1))
            decimal accountLimit = limitsDictionary.TryGetValue(transaction.AccountId, out var customLimit)
                ? customLimit
                : DefaultLimit;

            // 6. Obtener lo acumulado hasta el momento
            dailySpentByAccount.TryGetValue(transaction.AccountId, out decimal currentSpent);

            // 7. Regla 3: Validar si excede el límite
            if (currentSpent + transaction.Amount > accountLimit)
            {
                flaggedTransactions.Add(transaction.TransactionId);
            }
            else
            {
                // Aprobada: Actualizar acumulado
                dailySpentByAccount[transaction.AccountId] = currentSpent + transaction.Amount;
            }
        }

        return flaggedTransactions;
    }
    }