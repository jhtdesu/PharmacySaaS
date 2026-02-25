namespace Inventory.Domain.Entities;

public class Medicine : BaseData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty; // Mã quản lý kho (ví dụ: PARA-500)
    public string ActiveIngredient { get; set; } = string.Empty; // Hoạt chất (ví dụ: Paracetamol)
    public string Unit { get; set; } = string.Empty; // Đơn vị tính (Viên, Chai, Vỉ)

    // Quan hệ 1-n: Một loại thuốc có nhiều lô hàng khác nhau
    public List<MedicineBatch> Batches { get; private set; } = new();

    // Logic nghiệp vụ: Tính tổng tồn kho từ tất cả các lô
    public int GetTotalStock() => Batches.Sum(b => b.CurrentQuantity);
}