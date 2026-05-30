using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Farmacontrol.Core.Model.ProductEntity;
using Farmacontrol.Core.Services;
using Farmacontrol.Model;

namespace Farmacontrol.Desktop.States;

public partial class AddProductState : ObservableObject
{
    private readonly InventoryService _inventoryService;
    private Product? _editingProduct;
    
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

    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string _title = "Nuevo Producto";

    public bool IsMedicine => SelectedProductType == "Medicamento";
    public bool IsSupply => SelectedProductType == "Suministro";
    public bool IsSupplement => SelectedProductType == "Suplemento";
    public bool IsCosmetic => SelectedProductType == "Cosmético";
    public bool IsEditing => _editingProduct != null;

    public AddProductState(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public void PrepareForAdd()
    {
        _editingProduct = null;
        Title = "Nuevo Producto";
        Reset();
    }

    public void PrepareForEdit(Product product)
    {
        _editingProduct = product;
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

            Product product = CreateProductInstance();
            PopulateCommonFields(product);
            PopulateTypeSpecificFields(product);

            if (IsEditing && _editingProduct != null)
            {
                _inventoryService.UpdateProduct(product);
            }
            else
            {
                _inventoryService.AddProduct(product);
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
        product.Code = Code;
        product.Name = Name;
        product.Price = decimal.Parse(Price);
        product.Stock = int.Parse(Stock);
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

        if (!IsEditing || _editingProduct == null) return;
        
        product.Code = _editingProduct.Code;
        product.CreatedAt = _editingProduct.CreatedAt;
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

    private void Reset()
    {
        Name = Code = Price = Stock = MinimumStock = Barcode = Location = Laboratory = string.Empty;
        Subcategory = Ingredients = Tags = string.Empty;
        ActivePrinciple = Concentration = Presentation = Brand = Type = Size = Material = RecommendedDosage = string.Empty;
        RequiresPrescription = IsControlled = IsSterile = Hypoallergenic = false;
        SelectedProductType = "Medicamento";
        SelectedFormat = SupplementFormat.Capsule;
        ErrorMessage = string.Empty;
        IsSaving = false;
    }
}