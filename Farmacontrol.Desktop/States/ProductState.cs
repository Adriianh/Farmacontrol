using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Model.ProductEntity;
using Farmacontrol.Core.Services;
using Farmacontrol.Model;

namespace Farmacontrol.Desktop.States;

public partial class ProductState(InventoryService inventoryService) : ObservableObject
{
    private Product? _editingProduct;
    private List<Batch> _originalBatches = [];

    public List<string> ProductTypes { get; } = ["Medicamento", "Suministro", "Suplemento", "Cosmético"];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMedicine), nameof(IsSupply), nameof(IsSupplement), nameof(IsCosmetic))]
    private string _selectedProductType = "Medicamento";

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _code = string.Empty;
    [ObservableProperty] private string _price = string.Empty;
    [ObservableProperty] private string _stock = string.Empty;
    [ObservableProperty] private string _minimumStock = string.Empty;
    [ObservableProperty] private string _barcode = string.Empty;
    [ObservableProperty] private string _location = string.Empty;
    [ObservableProperty] private string _laboratory = string.Empty;
    [ObservableProperty] private string _subcategory = string.Empty;
    [ObservableProperty] private string _ingredients = string.Empty;
    [ObservableProperty] private string _tags = string.Empty;

    [ObservableProperty] private string _activePrinciple = string.Empty;
    [ObservableProperty] private bool _requiresPrescription;
    [ObservableProperty] private string _concentration = string.Empty;
    [ObservableProperty] private string _presentation = string.Empty;
    [ObservableProperty] private bool _isControlled;

    [ObservableProperty] private string _brand = string.Empty;
    [ObservableProperty] private string _type = string.Empty;
    [ObservableProperty] private string _size = string.Empty;
    [ObservableProperty] private string _material = string.Empty;
    [ObservableProperty] private bool _isSterile;
    [ObservableProperty] private bool _hypoallergenic;
    [ObservableProperty] private string _recommendedDosage = string.Empty;
    [ObservableProperty] private SupplementFormat _selectedFormat = SupplementFormat.Capsule;

    [ObservableProperty] private bool _enableBatches;
    [ObservableProperty] private bool _enableSuppliers;

    [ObservableProperty] private string _batchLotCode = string.Empty;
    [ObservableProperty] private string _batchQuantity = string.Empty;
    [ObservableProperty] private DateTime? _batchManufacturingDate = DateTime.Today;
    [ObservableProperty] private DateTime? _batchExpirationDate = DateTime.Today.AddYears(1);
    [ObservableProperty] private string _batchUnitCost = string.Empty;
    [ObservableProperty] private bool _showBatchForm;
    [ObservableProperty] private string _batchesInfo = string.Empty;

    [ObservableProperty]
    private ObservableCollection<(string LotCode, int Quantity, DateTime MfgDate, DateTime ExpDate, decimal UnitCost)>
        _batches = new();

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSuppliers))]
    private List<Supplier> _availableSuppliers = new();

    [ObservableProperty] private List<Supplier> _selectedSuppliers = new();
    [ObservableProperty] private string _suppliersInfo = string.Empty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string _errorMessage = string.Empty;

    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string _title = "Nuevo Producto";

    public bool IsMedicine => SelectedProductType == "Medicamento";
    public bool IsSupply => SelectedProductType == "Suministro";
    public bool IsSupplement => SelectedProductType == "Suplemento";
    public bool IsCosmetic => SelectedProductType == "Cosmético";
    private bool IsEditing => _editingProduct != null;
    public bool HasBatches => Batches.Count > 0;
    public bool HasSuppliers => SelectedSuppliers.Count > 0;
    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public void PrepareForAdd()
    {
        _editingProduct = null;
        Title = "Nuevo Producto";
        Reset();
    }

    public void PrepareForEdit(Product product)
    {
        _editingProduct = product;
        _originalBatches = new List<Batch>(product.Batches);
        Title = "Editar Producto";
        Reset();

        Name = product.Name;
        Code = product.Code;
        Price = product.Price.ToString("F2");
        Stock = product.Stock.ToString();
        MinimumStock = product.MinimumStock.ToString();
        Barcode = product.Barcode ?? string.Empty;
        Location = product.Location ?? string.Empty;
        Laboratory = product.Laboratory ?? string.Empty;
        Subcategory = product.Subcategory ?? string.Empty;
        Ingredients = string.Join(", ", product.Ingredients);
        Tags = string.Join(", ", product.Tags);

        EnableBatches = product.Batches.Count > 0;

        foreach (var batch in product.Batches)
        {
            Batches.Add((
                batch.LotCode,
                batch.Quantity,
                batch.ManufacturingDate,
                batch.ExpirationDate,
                batch.UnitCost
            ));
        }

        OnPropertyChanged(nameof(HasBatches));
        UpdateBatchesInfo();

        switch (product)
        {
            case Medicine medicine:
                SelectedProductType = "Medicamento";
                ActivePrinciple = medicine.ActivePrinciple;
                Concentration = medicine.Concentration ?? string.Empty;
                Presentation = medicine.Presentation ?? string.Empty;
                RequiresPrescription = medicine.RequiresPrescription;
                IsControlled = medicine.IsControlled;
                break;

            case Supply supply:
                SelectedProductType = "Suministro";
                Brand = supply.Brand;
                Type = supply.Type;
                Size = supply.Size ?? string.Empty;
                Material = supply.Material ?? string.Empty;
                IsSterile = supply.IsSterile;
                break;

            case Supplement supplement:
                SelectedProductType = "Suplemento";
                ActivePrinciple = supplement.ActivePrinciple;
                Type = supplement.Type;
                SelectedFormat = supplement.Format;
                Concentration = supplement.Concentration ?? string.Empty;
                RecommendedDosage = supplement.RecommendedDosage ?? string.Empty;
                break;

            case Cosmetic cosmetic:
                SelectedProductType = "Cosmético";
                Brand = cosmetic.Brand;
                Type = cosmetic.Type;
                Presentation = cosmetic.Presentation ?? string.Empty;
                Hypoallergenic = cosmetic.Hypoallergenic;
                break;
        }
    }

    public void SaveProduct()
    {
        try
        {
            ErrorMessage = string.Empty;
            IsSaving = true;

            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Code))
            {
                ErrorMessage = "Nombre y Código son requeridos";
                return;
            }

            if (!decimal.TryParse(Price, out var price) || price < 0)
            {
                ErrorMessage = "Precio inválido";
                return;
            }

            if (!int.TryParse(Stock, out var stock) || stock < 0)
            {
                ErrorMessage = "Stock inicial inválido";
                return;
            }

            if (!int.TryParse(MinimumStock, out var minimumStock) || minimumStock < 0)
            {
                ErrorMessage = "Stock mínimo inválido";
                return;
            }

            Product product;

            if (IsEditing && _editingProduct != null)
            {
                product = _editingProduct;
            }
            else
            {
                product = CreateProductInstance();
            }

            PopulateCommonFields(product);
            PopulateTypeSpecificFields(product);

            if (EnableSuppliers)
            {
                product.Suppliers.Clear();
                foreach (var supplier in SelectedSuppliers)
                {
                    product.Suppliers.Add(supplier);
                }
            }

            if (IsEditing && _editingProduct != null)
            {
                inventoryService.UpdateProduct(product);
            }
            else
            {
                inventoryService.AddProduct(product);
            }

            Reset();
            _editingProduct = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private Product CreateProductInstance()
    {
        return SelectedProductType switch
        {
            "Medicamento" => new Medicine(),
            "Suministro" => new Supply(),
            "Suplemento" => new Supplement(),
            "Cosmético" => new Cosmetic(),
            _ => new Medicine()
        };
    }

    private void PopulateCommonFields(Product product)
    {
        if (!IsEditing)
        {
            product.Code = Code;
        }

        product.Name = Name;
        product.Price = decimal.Parse(Price);
        product.MinimumStock = int.Parse(MinimumStock);
        product.Barcode = string.IsNullOrWhiteSpace(Barcode) ? null : Barcode;
        product.Location = string.IsNullOrWhiteSpace(Location) ? null : Location;
        product.Laboratory = string.IsNullOrWhiteSpace(Laboratory) ? null : Laboratory;
        product.Subcategory = string.IsNullOrWhiteSpace(Subcategory) ? null : Subcategory;

        product.Ingredients = Ingredients.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        product.Tags = Tags.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (EnableBatches)
        {
            var processedBatches = new List<Batch>();

            foreach (var formBatch in Batches)
            {
                var existingBatch = _originalBatches.FirstOrDefault(b =>
                    b.LotCode == formBatch.LotCode &&
                    b.ManufacturingDate == formBatch.MfgDate);

                if (existingBatch != null)
                {
                    existingBatch.Quantity = formBatch.Quantity;
                    existingBatch.ExpirationDate = formBatch.ExpDate;
                    existingBatch.UnitCost = formBatch.UnitCost;
                    processedBatches.Add(existingBatch);
                }
                else
                {
                    processedBatches.Add(new Batch(
                        product.Code,
                        formBatch.LotCode,
                        formBatch.Quantity,
                        formBatch.ExpDate,
                        formBatch.MfgDate)
                    {
                        UnitCost = formBatch.UnitCost
                    });
                }
            }

            product.Batches.Clear();
            foreach (var batch in processedBatches)
            {
                product.Batches.Add(batch);
            }

            product.Stock = Batches.Sum(b => b.Quantity);
        }
        else
        {
            product.Stock = int.Parse(Stock);

            if (IsEditing && _editingProduct != null)
            {
                product.Batches.Clear();
                foreach (var batch in _editingProduct.Batches)
                {
                    product.Batches.Add(batch);
                }
            }
        }
    }

    private void PopulateTypeSpecificFields(Product product)
    {
        switch (product)
        {
            case Medicine medicine:
                medicine.ActivePrinciple = ActivePrinciple;
                medicine.Concentration = string.IsNullOrWhiteSpace(Concentration) ? null : Concentration;
                medicine.Presentation = string.IsNullOrWhiteSpace(Presentation) ? null : Presentation;
                medicine.RequiresPrescription = RequiresPrescription;
                medicine.IsControlled = IsControlled;
                break;

            case Supply supply:
                supply.Brand = Brand;
                supply.Type = Type;
                supply.Size = string.IsNullOrWhiteSpace(Size) ? null : Size;
                supply.Material = string.IsNullOrWhiteSpace(Material) ? null : Material;
                supply.IsSterile = IsSterile;
                break;

            case Supplement supplement:
                supplement.ActivePrinciple = ActivePrinciple;
                supplement.Type = Type;
                supplement.Format = SelectedFormat;
                supplement.Concentration = string.IsNullOrWhiteSpace(Concentration) ? null : Concentration;
                supplement.RecommendedDosage = string.IsNullOrWhiteSpace(RecommendedDosage) ? null : RecommendedDosage;
                break;

            case Cosmetic cosmetic:
                cosmetic.Brand = Brand;
                cosmetic.Type = Type;
                cosmetic.Presentation = string.IsNullOrWhiteSpace(Presentation) ? null : Presentation;
                cosmetic.Hypoallergenic = Hypoallergenic;
                break;
        }
    }

    public void AddBatch()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(BatchLotCode))
            {
                ErrorMessage = "Número de lote es requerido";
                return;
            }

            if (!int.TryParse(BatchQuantity, out var qty) || qty <= 0)
            {
                ErrorMessage = "Cantidad de lote inválida";
                return;
            }

            if (!BatchManufacturingDate.HasValue)
            {
                ErrorMessage = "Fecha de fabricación inválida";
                return;
            }

            if (!BatchExpirationDate.HasValue)
            {
                ErrorMessage = "Fecha de expiración inválida";
                return;
            }

            var mfgDate = BatchManufacturingDate.Value;
            var expDate = BatchExpirationDate.Value;

            if (expDate <= mfgDate)
            {
                ErrorMessage = "La fecha de expiración debe ser posterior a la de fabricación";
                return;
            }

            if (expDate < DateTime.Today)
            {
                ErrorMessage = "La fecha de expiración no puede ser una fecha en el pasado";
                return;
            }

            if (!string.IsNullOrWhiteSpace(BatchUnitCost) &&
                (!decimal.TryParse(BatchUnitCost, out var parsedUnitCost) || parsedUnitCost < 0))
            {
                ErrorMessage = "Costo unitario inválido";
                return;
            }

            var unitCost = string.IsNullOrWhiteSpace(BatchUnitCost) ? 0m : decimal.Parse(BatchUnitCost);

            var newBatch = (BatchLotCode.Trim(), qty, mfgDate, expDate, unitCost);
            Batches.Add(newBatch);

            OnPropertyChanged(nameof(HasBatches));

            UpdateBatchesInfo();
            ClearBatchForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al agregar lote: {ex.Message}";
        }
    }

    public void RemoveBatch(int index)
    {
        if (index >= 0 && index < Batches.Count)
        {
            Batches.RemoveAt(index);
            OnPropertyChanged(nameof(HasBatches));
            UpdateBatchesInfo();
        }
    }

    public void ToggleBatchForm()
    {
        ShowBatchForm = !ShowBatchForm;
        if (!ShowBatchForm)
        {
            ClearBatchForm();
        }
    }

    private void ClearBatchForm()
    {
        BatchLotCode = string.Empty;
        BatchQuantity = string.Empty;
        BatchManufacturingDate = DateTime.Today;
        BatchExpirationDate = DateTime.Today.AddYears(1);
        BatchUnitCost = string.Empty;
    }

    private void UpdateBatchesInfo()
    {
        if (Batches.Count == 0)
        {
            BatchesInfo = string.Empty;
            return;
        }

        var batchList = string.Join("\n", Batches.Select((b, i) =>
            $"Lote {i + 1}: {b.LotCode} - {b.Quantity} unidades (Exp: {b.ExpDate:yyyy-MM-dd})"));

        BatchesInfo = $"Lotes agregados ({Batches.Count}):\n{batchList}";
    }

    public void SetAvailableSuppliers(List<Supplier> suppliers)
    {
        AvailableSuppliers = suppliers;
    }

    public void ToggleSupplier(Supplier supplier)
    {
        if (SelectedSuppliers.Contains(supplier))
        {
            SelectedSuppliers.Remove(supplier);
        }
        else
        {
            SelectedSuppliers.Add(supplier);
        }

        UpdateSuppliersInfo();
    }

    private void UpdateSuppliersInfo()
    {
        if (SelectedSuppliers.Count == 0)
        {
            SuppliersInfo = string.Empty;
            return;
        }

        var supplierList = string.Join(", ", SelectedSuppliers.Select(s => s.Name));
        SuppliersInfo = $"Proveedores: {supplierList}";
    }

    private void Reset()
    {
        Name = Code = Price = Stock = MinimumStock = Barcode = Location = Laboratory = string.Empty;
        Subcategory = Ingredients = Tags = string.Empty;
        ActivePrinciple =
            Concentration = Presentation = Brand = Type = Size = Material = RecommendedDosage = string.Empty;
        RequiresPrescription = IsControlled = IsSterile = Hypoallergenic = false;
        SelectedProductType = "Medicamento";
        SelectedFormat = SupplementFormat.Capsule;
        ErrorMessage = string.Empty;
        IsSaving = false;
        ShowBatchForm = false;
        EnableBatches = false;
        EnableSuppliers = false;
        ClearBatchForm();
        Batches.Clear();
        _originalBatches.Clear();
        OnPropertyChanged(nameof(HasBatches));
        SelectedSuppliers.Clear();
        UpdateBatchesInfo();
        UpdateSuppliersInfo();
    }
}