```markdown
# 💳 Real-Time Transaction & Fraud Detector (C# / .NET)

¡Hola! 👋 Bienvenido a este repositorio. Este proyecto es una solución a un reto técnico típico para posiciones de desarrollo **C# en el sector Fintech**. 

El objetivo es procesar un flujo de transacciones financieras en tiempo real, validando reglas de negocio, límites de crédito diarios y alertas básicas anti-fraude.

---

## 📌 El Desafío

El sistema debe analizar un conjunto de transacciones (`List<Transaction>`), compararlas contra una lista de límites configurados por cuenta (`List<AccountLimit>`) y evaluar si son válidas para la fecha de procesamiento dada (`processingDate`).

### ⚙️ Reglas de Negocio

Una transacción debe ser **rechazada o marcada como sospechosa** si cumple cualquiera de los siguientes criterios:

1. **Fecha Inválida:** La fecha de la transacción no coincide con el día de procesamiento (`processingDate`).
2. **Monto Inválido:** El monto es menor o igual a cero (`Amount <= 0`).
3. **Alerta Anti-Fraude:** La transacción individual supera los **$2,000.00** (independientemente del límite de la cuenta).
4. **Exceso de Límite Diario:** El acumulado diario de transacciones *aprobadas* de una cuenta excede su límite asignado.
   * *Nota:* Si la cuenta no tiene un límite registrado, se aplica un límite por defecto de **$500.00**.

---

## 💡 Enfoque de Solución & Arquitectura

Para resolver este problema, no solo busqué que el código cumpliera con los test funcionales, sino que prioricé la **eficiencia en memoria**, la **escalabilidad** y la **legibilidad del código**.

### 🛠️ Key Technical Highlights:

* **Búsquedas eficientes $O(1)$:** En lugar de iterar la lista de límites con LINQ dentro del bucle principal (lo cual elevaría la complejidad a $O(N \times M)$), convertí la lista en un `Dictionary<string, decimal>` antes de procesar. Esto permite obtener los límites mediante `TryGetValue` en tiempo constante $O(1)$.
* **Control del acumulado en tiempo real:** Usé un diccionario auxiliar (`dailySpentByAccount`) para rastrear el gasto diario acumulado por cuenta. Solo las transacciones aprobadas actualizan este saldo.
* **Uso de Guard Clauses (Early Exit):** Desestructuré los condicionales anidados usando `continue` al detectar errores tempranos. Esto reduce la complejidad cognitiva y hace el código fácil de mantener.
* **Precisión Financiera:** Todos los montos monetarios utilizan el tipo `decimal` para evitar errores de redondeo por coma flotante (`double`/`float`).

---

## 🚀 Cómo Ejecutar el Proyecto

### Requisitos Próximos
* [.NET 8 Core SDK](https://dotnet.microsoft.com/download) o superior.

### Pasos

1. Clona este repositorio:
   ```bash
   git clone [https://github.com/TU_USUARIO/fintech-transaction-detector.git](https://github.com/TU_USUARIO/fintech-transaction-detector.git)

```

2. Entra a la carpeta del proyecto:
```bash
cd fintech-transaction-detector

```


3. Ejecuta el proyecto:
```bash
dotnet run

```



---

## 💻 Código Principal

```csharp
public static List<string> ProcessAndDetectFraud(
    List<Transaction> transactions,
    List<AccountLimit> limits,
    DateTime processingDate)
{
    if (transactions == null) return new List<string>();

    var flaggedTransactions = new List<string>();
    var dailySpentByAccount = new Dictionary<string, decimal>();

    // Mapeo O(1) de límites
    var limitsDictionary = limits?.ToDictionary(l => l.AccountId, l => l.DailyLimit) 
                           ?? new Dictionary<string, decimal>();

    const decimal DefaultLimit = 500.00m;
    const decimal MaxSingleTransactionLimit = 2000.00m;

    foreach (var transaction in transactions)
    {
        // 1. Validaciones básicas de entrada
        if (transaction.Timestamp.Date != processingDate.Date || transaction.Amount <= 0)
        {
            flaggedTransactions.Add(transaction.TransactionId);
            continue;
        }

        // 2. Alerta Anti-Fraude
        if (transaction.Amount > MaxSingleTransactionLimit)
        {
            flaggedTransactions.Add(transaction.TransactionId);
            continue;
        }

        // 3. Evaluación de Límite Diario acumulado
        decimal accountLimit = limitsDictionary.TryGetValue(transaction.AccountId, out var customLimit) 
            ? customLimit 
            : DefaultLimit;

        dailySpentByAccount.TryGetValue(transaction.AccountId, out decimal currentSpent);

        if (currentSpent + transaction.Amount > accountLimit)
        {
            flaggedTransactions.Add(transaction.TransactionId);
        }
        else
        {
            dailySpentByAccount[transaction.AccountId] = currentSpent + transaction.Amount;
        }
    }

    return flaggedTransactions;
}

```

---

✉️ *Proyecto desarrollado como práctica de preparación para entrevistas técnicas C# en el sector Fintech.*

```

---

### Tips adicionales para tu repositorio:
1. **Nombre del repositorio sugerido:** `fintech-transaction-evaluator` o `csharp-fraud-detection-challenge`.
2. **Añade `Topics` en GitHub:** Agrega etiquetas como `csharp`, `dotnet`, `fintech`, `linq`, `clean-code`, `algorithms`. Esto mejora la visibilidad de tu perfil.

<ElicitationsGroup message="¿Te gusta esta estructura para tu README o prefieres agregarle algo más?">

  <Elicitation label="¿Cómo puedo subir este código a GitHub paso a paso usando Git?" query="Dame los comandos de consola Git paso a paso para crear un repositorio local, hacer commit y subirlo a GitHub desde cero."/>

  <Elicitation label="Agregar pruebas unitarias (xUnit) al repositorio para hacerlo más pro" query="Muéstrame cómo estructurar pruebas unitarias con xUnit para este ejercicio de transacciones y así subirlas también al repositorio."/>
</ElicitationsGroup>

```