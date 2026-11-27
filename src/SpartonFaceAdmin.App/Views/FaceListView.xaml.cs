using System;
using System.Windows;
using System.Windows.Controls;
using SpartonFaceAdmin.App.ViewModels;

namespace SpartonFaceAdmin.App.Views;

public partial class FaceListView : UserControl
{
    private TextBox? _editableTextBox;
    private bool _isSyncing;

    public FaceListView()
    {
        InitializeComponent();
        Loaded += FaceListView_Loaded;
    }

    private void FaceListView_Loaded(object sender, RoutedEventArgs e)
    {
        AttachEditableTextBox();
        UpdatePlaceholder();
    }

    private void AttachEditableTextBox()
    {
        if (_editableTextBox != null)
            return;

        CbEmployeeSearch.ApplyTemplate();
        if (CbEmployeeSearch.Template?.FindName("PART_EditableTextBox", CbEmployeeSearch) is TextBox tb)
        {
            _editableTextBox = tb;
            _editableTextBox.TextChanged += EditableTextBox_TextChanged;
            UpdatePlaceholder();
        }
    }

    private void EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdatePlaceholder();

        if (_isSyncing || DataContext is not FaceListViewModel vm || _editableTextBox == null)
            return;

        string typed = _editableTextBox.Text;
        int caret = _editableTextBox.CaretIndex;

        // Ignore programmatic text sync when SelectedEmployee's label is applied
        if (vm.SelectedEmployee != null &&
            string.Equals(typed, vm.SelectedEmployee.DisplayLabel, StringComparison.Ordinal))
        {
            return;
        }

        _isSyncing = true;
        try
        {
            vm.SearchText = typed;

            if (!CbEmployeeSearch.IsDropDownOpen)
                CbEmployeeSearch.IsDropDownOpen = true;

            // Keep user's typed text & caret intact if WPF Selector cleared text on ItemsSource update
            if (_editableTextBox.Text != typed)
            {
                _editableTextBox.Text = typed;
                _editableTextBox.CaretIndex = Math.Min(caret, typed.Length);
            }
        }
        finally
        {
            _isSyncing = false;
            UpdatePlaceholder();
        }
    }

    private void CbEmployeeSearch_DropDownOpened(object sender, EventArgs e)
    {
        AttachEditableTextBox();

        if (_isSyncing || DataContext is not FaceListViewModel vm)
            return;

        // When user clicks dropdown while an employee is already selected, show full list immediately
        string currentText = _editableTextBox?.Text ?? CbEmployeeSearch.Text ?? string.Empty;
        if (vm.SelectedEmployee != null &&
            string.Equals(currentText, vm.SelectedEmployee.DisplayLabel, StringComparison.Ordinal) &&
            !string.IsNullOrEmpty(vm.SearchText))
        {
            _isSyncing = true;
            try
            {
                vm.SearchText = string.Empty;
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }

    private void CbEmployeeSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isSyncing)
            return;

        _isSyncing = true;
        try
        {
            UpdatePlaceholder();
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void UpdatePlaceholder()
    {
        string text = _editableTextBox?.Text ?? CbEmployeeSearch.Text ?? string.Empty;
        TxtEmployeePlaceholder.Visibility = string.IsNullOrEmpty(text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
