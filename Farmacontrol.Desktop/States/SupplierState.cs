using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Repository;
using Farmacontrol.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Desktop.States;

public partial class SupplierState : ObservableObject
{
    private readonly InventoryService _inventoryService;
    private readonly AppDbContext _db;
    private List<Supplier> _baseSuppliers = [];

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<Supplier> _filteredSuppliers = [];
    [ObservableProperty] private bool _isModalOpen;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ModalTitle))]
    private bool _isEditing;

    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private string _code = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _leadTimeDays = string.Empty;
    [ObservableProperty] private string _taxId = string.Empty;
    [ObservableProperty] private string _paymentTerms = string.Empty;
    [ObservableProperty] private string _contactName = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private bool _isActive = true;

    private Supplier? _editingSupplier;

    public string ModalTitle => IsEditing ? "✏️ Editar Proveedor" : "🏢 Nuevo Proveedor";
    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public SupplierState(InventoryService inventoryService, AppDbContext db)
    {
        _inventoryService = inventoryService;
        _db = db;
        LoadSuppliers();
    }

    partial void OnSearchTextChanged(string value) => UpdateFilteredSuppliers();

    private void LoadSuppliers()
    {
        try
        {
            _baseSuppliers = _db.Suppliers
                .Include(s => s.Products)
                .ToList();

            UpdateFilteredSuppliers();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar proveedores: {ex.Message}";
        }
    }

    private void UpdateFilteredSuppliers()
    {
        var filtered = _baseSuppliers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(s =>
                s.Name.ToLower().Contains(search) ||
                s.Code.ToLower().Contains(search) ||
                (s.ContactName != null && s.ContactName.ToLower().Contains(search)));
        }

        FilteredSuppliers = new ObservableCollection<Supplier>(filtered.OrderBy(s => s.Name));
    }

    public void PrepareAddSupplier()
    {
        ResetForm();
        IsEditing = false;
        IsModalOpen = true;
    }

    public void PrepareEditSupplier(Supplier supplier)
    {
        ResetForm();
        _editingSupplier = supplier;
        IsEditing = true;

        Code = supplier.Code;
        Name = supplier.Name;
        PhoneNumber = supplier.PhoneNumber;
        Email = supplier.Email;
        LeadTimeDays = supplier.LeadTimeDays.ToString();
        TaxId = supplier.TaxId ?? string.Empty;
        PaymentTerms = supplier.PaymentTerms ?? string.Empty;
        ContactName = supplier.ContactName ?? string.Empty;
        Address = supplier.Address ?? string.Empty;
        IsActive = supplier.IsActive;

        IsModalOpen = true;
    }

    public void SaveSupplier()
    {
        if (string.IsNullOrWhiteSpace(Code) || string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(PhoneNumber) || string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Los campos Código, Nombre, Teléfono y Correo son obligatorios.";
            return;
        }

        if (!int.TryParse(LeadTimeDays, out int days) || days < 0)
        {
            ErrorMessage = "El tiempo de entrega debe ser un número entero válido (días).";
            return;
        }

        try
        {
            if (IsEditing && _editingSupplier != null)
            {
                _editingSupplier.Name = Name;
                _editingSupplier.PhoneNumber = PhoneNumber;
                _editingSupplier.Email = Email;
                _editingSupplier.LeadTimeDays = days;
                _editingSupplier.TaxId = string.IsNullOrWhiteSpace(TaxId) ? null : TaxId;
                _editingSupplier.PaymentTerms = string.IsNullOrWhiteSpace(PaymentTerms) ? null : PaymentTerms;
                _editingSupplier.ContactName = string.IsNullOrWhiteSpace(ContactName) ? null : ContactName;
                _editingSupplier.Address = string.IsNullOrWhiteSpace(Address) ? null : Address;
                _editingSupplier.IsActive = IsActive;

                _db.Suppliers.Update(_editingSupplier);
            }
            else
            {
                if (_db.Suppliers.Any(s => s.Code == Code))
                {
                    ErrorMessage = "Ya existe un proveedor registrado con ese código.";
                    return;
                }

                var newSupplier = new Supplier(Code, Name, PhoneNumber, Email, days,
                    string.IsNullOrWhiteSpace(ContactName) ? null : ContactName,
                    string.IsNullOrWhiteSpace(Address) ? null : Address)
                {
                    TaxId = string.IsNullOrWhiteSpace(TaxId) ? null : TaxId,
                    PaymentTerms = string.IsNullOrWhiteSpace(PaymentTerms) ? null : PaymentTerms,
                    IsActive = IsActive
                };

                _db.Suppliers.Add(newSupplier);
            }

            _db.SaveChanges();
            CloseModal();
            LoadSuppliers();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
        }
    }

    public void DeleteSupplier(Supplier supplier)
    {
        try
        {
            ErrorMessage = string.Empty;

            if (supplier.Products.Count > 0)
            {
                var productCount = supplier.Products.Count;
                var productWord = productCount == 1 ? "producto" : "productos";

                ErrorMessage = $"⚠️ No se puede eliminar '{supplier.Name}'. " +
                               $"Tiene {productCount} {productWord} bajo su distribución. " +
                               "Desasigne el proveedor de esos productos en el inventario antes de continuar.";
                return;
            }

            _db.Suppliers.Remove(supplier);
            _db.SaveChanges();

            LoadSuppliers();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error inesperado al intentar eliminar el proveedor: {ex.Message}";
        }
    }

    public void CloseModal()
    {
        IsModalOpen = false;
        ResetForm();
    }

    private void ResetForm()
    {
        Code = Name = PhoneNumber = Email = LeadTimeDays = string.Empty;
        TaxId = PaymentTerms = ContactName = Address = string.Empty;
        IsActive = true;
        ErrorMessage = string.Empty;
        _editingSupplier = null;
    }
}