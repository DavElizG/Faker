using Api.Domain.Entities;
using Api.Domain.Interfaces;

public class FailedPurchaseStore : IFailedPurchaseStore
{
    private readonly List<Purchase> _failedPurchases = new List<Purchase>();
    private readonly object _lock = new object();

    public FailedPurchaseStore()
    {
        Console.WriteLine($"FailedPurchaseStore instance created with hash code: {this.GetHashCode()}");
    }

    public void AddFailedPurchase(Purchase purchase)
    {
        lock (_lock)
        {
            if (!_failedPurchases.Any(p => p.Id == purchase.Id))
            {
                _failedPurchases.Add(purchase);
            }
        }
    }

    public Purchase GetFailedPurchaseById(Guid purchaseId)
    {
        lock (_lock)
        {
            return _failedPurchases.FirstOrDefault(p => p.Id == purchaseId);
        }
    }

    public List<Purchase> GetAllFailedPurchases()
    {
        lock (_lock)
        {
            return _failedPurchases.ToList();
        }
    }

    public void RemoveFailedPurchase(Purchase purchase)
    {
        lock (_lock)
        {
            _failedPurchases.Remove(purchase);
        }
    }
}