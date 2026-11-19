using System.Data;
using MySqlConnector;

namespace pruebaaa
{
    public partial class Form1 : Form
    {
        private readonly TextBox serverInput = new() { Text = "localhost", Width = 120 };
        private readonly TextBox portInput = new() { Text = "3306", Width = 65 };
        private readonly TextBox databaseInput = new() { Text = "tienda_ventas", Width = 120 };
        private readonly TextBox userInput = new() { Text = "root", Width = 100 };
        private readonly TextBox passwordInput = new() { Width = 100, UseSystemPasswordChar = true };
        private readonly DataGridView clientsGrid = CreateGrid();
        private readonly DataGridView productsGrid = CreateGrid();
        private readonly DataGridView salesGrid = CreateGrid();
        private readonly TextBox clientNameInput = new() { Width = 170 };
        private readonly TextBox clientPhoneInput = new() { Width = 130 };
        private readonly TextBox clientEmailInput = new() { Width = 180 };
        private readonly TextBox productNameInput = new() { Width = 170 };
        private readonly TextBox productPriceInput = new() { Width = 100 };
        private readonly TextBox productStockInput = new() { Width = 90 };
        private readonly ComboBox saleClientInput = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox saleProductInput = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker saleDateInput = new() { Width = 130, Format = DateTimePickerFormat.Short };
        private readonly NumericUpDown saleQuantityInput = new() { Width = 75, Minimum = 1, Maximum = 100000, Value = 1 };

        public Form1()
        {
            InitializeComponent();
            Text = "Sistema de Ventas de Tienda";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 600);
            ClientSize = new Size(1100, 720);
            BuildInterface();
        }

        private string ConnectionString => new MySqlConnectionStringBuilder
        {
            Server = serverInput.Text.Trim(),
            Port = uint.TryParse(portInput.Text, out var port) ? port : 3306,
            Database = databaseInput.Text.Trim(),
            UserID = userInput.Text.Trim(),
            Password = passwordInput.Text,
            ConnectionTimeout = 5
        }.ConnectionString;

        private void BuildInterface()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var connectionPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 10) };
            connectionPanel.Controls.AddRange(new Control[]
            {
                LabeledControl("Servidor", serverInput), LabeledControl("Puerto", portInput),
                LabeledControl("Base de datos", databaseInput), LabeledControl("Usuario", userInput),
                LabeledControl("Contraseña", passwordInput), CreateButton("Conectar / actualizar", (_, _) => ConnectAndRefresh())
            });
            root.Controls.Add(connectionPanel, 0, 0);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(BuildClientsTab());
            tabs.TabPages.Add(BuildProductsTab());
            tabs.TabPages.Add(BuildSalesTab());
            root.Controls.Add(tabs, 0, 1);
            Controls.Add(root);
        }

        private TabPage BuildClientsTab()
        {
            var page = new TabPage("Clientes");
            var panel = CreateTabLayout();
            var fields = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            fields.Controls.AddRange(new Control[]
            {
                LabeledControl("Nombre", clientNameInput), LabeledControl("Teléfono", clientPhoneInput),
                LabeledControl("Correo", clientEmailInput), CreateButton("Insertar cliente", (_, _) => InsertClient()),
                CreateButton("Eliminar seleccionado", (_, _) => DeleteSelected(clientsGrid, "cliente", "DELETE FROM Clientes WHERE IdCliente = @id", "IdCliente", RefreshClients))
            });
            panel.Controls.Add(fields, 0, 0);
            panel.Controls.Add(clientsGrid, 0, 1);
            page.Controls.Add(panel);
            return page;
        }

        private TabPage BuildProductsTab()
        {
            var page = new TabPage("Productos");
            var panel = CreateTabLayout();
            var fields = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            fields.Controls.AddRange(new Control[]
            {
                LabeledControl("Producto", productNameInput), LabeledControl("Precio", productPriceInput),
                LabeledControl("Existencias", productStockInput), CreateButton("Insertar producto", (_, _) => InsertProduct()),
                CreateButton("Eliminar seleccionado", (_, _) => DeleteSelected(productsGrid, "producto", "DELETE FROM Productos WHERE IdProducto = @id", "IdProducto", RefreshProducts))
            });
            panel.Controls.Add(fields, 0, 0);
            panel.Controls.Add(productsGrid, 0, 1);
            page.Controls.Add(panel);
            return page;
        }

        private TabPage BuildSalesTab()
        {
            var page = new TabPage("Ventas");
            var panel = CreateTabLayout();
            var fields = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            fields.Controls.AddRange(new Control[]
            {
                LabeledControl("Cliente", saleClientInput), LabeledControl("Producto", saleProductInput),
                LabeledControl("Fecha de compra", saleDateInput), LabeledControl("Cantidad", saleQuantityInput),
                CreateButton("Registrar venta", (_, _) => InsertSale()),
                CreateButton("Eliminar venta", (_, _) => DeleteSelected(salesGrid, "venta", "DELETE FROM Ventas WHERE IdVenta = @id", "IdVenta", RefreshSales))
            });
            panel.Controls.Add(fields, 0, 0);
            panel.Controls.Add(salesGrid, 0, 1);
            page.Controls.Add(panel);
            return page;
        }

        private void ConnectAndRefresh()
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                RefreshAll();
                MessageBox.Show("Conexión exitosa. Datos actualizados.", "Sistema de Ventas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        private void RefreshAll()
        {
            RefreshClients();
            RefreshProducts();
            RefreshSales();
            LoadSaleOptions();
        }

        private void RefreshClients() => clientsGrid.DataSource = Query("SELECT IdCliente, Nombre, Telefono, Correo FROM Clientes ORDER BY Nombre");
        private void RefreshProducts() => productsGrid.DataSource = Query("SELECT IdProducto, Nombre, Precio, Existencias FROM Productos ORDER BY Nombre");
        private void RefreshSales() => salesGrid.DataSource = Query("SELECT v.IdVenta, c.Nombre AS Cliente, p.Nombre AS Producto, v.FechaCompra, v.Cantidad, v.PrecioUnitario, v.Cantidad * v.PrecioUnitario AS Total FROM Ventas v INNER JOIN Clientes c ON c.IdCliente = v.IdCliente INNER JOIN Productos p ON p.IdProducto = v.IdProducto ORDER BY v.FechaCompra DESC, v.IdVenta DESC");

        private void LoadSaleOptions()
        {
            saleClientInput.DataSource = Query("SELECT IdCliente, Nombre FROM Clientes ORDER BY Nombre");
            saleClientInput.DisplayMember = "Nombre";
            saleClientInput.ValueMember = "IdCliente";
            saleProductInput.DataSource = Query("SELECT IdProducto, Nombre FROM Productos WHERE Existencias > 0 ORDER BY Nombre");
            saleProductInput.DisplayMember = "Nombre";
            saleProductInput.ValueMember = "IdProducto";
        }

        private void InsertClient()
        {
            var name = clientNameInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Ingrese el nombre del cliente.", "Dato requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Execute("INSERT INTO Clientes (Nombre, Telefono, Correo) VALUES (@nombre, @telefono, @correo)",
                ("@nombre", name), ("@telefono", DbValue(clientPhoneInput.Text)), ("@correo", DbValue(clientEmailInput.Text)));
            clientNameInput.Clear();
            clientPhoneInput.Clear();
            clientEmailInput.Clear();
            RefreshClients();
            LoadSaleOptions();
        }

        private void InsertProduct()
        {
            if (string.IsNullOrWhiteSpace(productNameInput.Text) ||
                !decimal.TryParse(productPriceInput.Text, out var price) || price < 0 ||
                !int.TryParse(productStockInput.Text, out var stock) || stock < 0)
            {
                MessageBox.Show("Ingrese un nombre, un precio válido (0 o mayor) y existencias válidas (0 o mayores).", "Datos inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Execute("INSERT INTO Productos (Nombre, Precio, Existencias) VALUES (@nombre, @precio, @existencias)",
                ("@nombre", productNameInput.Text.Trim()), ("@precio", price), ("@existencias", stock));
            productNameInput.Clear();
            productPriceInput.Clear();
            productStockInput.Clear();
            RefreshProducts();
            LoadSaleOptions();
        }

        private void InsertSale()
        {
            if (saleClientInput.SelectedValue is null || saleProductInput.SelectedValue is null)
            {
                MessageBox.Show("Registre al menos un cliente y un producto con existencias.", "Datos requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RegisterSale(Convert.ToInt32(saleClientInput.SelectedValue), Convert.ToInt32(saleProductInput.SelectedValue),
                saleDateInput.Value.Date, decimal.ToInt32(saleQuantityInput.Value));

            RefreshProducts();
            RefreshSales();
            LoadSaleOptions();
        }

        private void DeleteSelected(DataGridView grid, string entityName, string sql, string idColumn, Action refresh)
        {
            if (grid.CurrentRow is null)
            {
                MessageBox.Show($"Seleccione {entityName} para eliminar.", "Selección requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var answer = MessageBox.Show($"¿Desea eliminar este registro de {entityName}?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes)
                return;

            var id = grid.CurrentRow.Cells[idColumn].Value;
            if (grid == salesGrid)
                DeleteSale(Convert.ToInt32(id));
            else
                Execute(sql, ("@id", id));

            refresh();
            if (grid != salesGrid)
                LoadSaleOptions();
        }

        private void RegisterSale(int clientId, int productId, DateTime purchaseDate, int quantity)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            decimal unitPrice;
            using (var stockCommand = new MySqlCommand(
                "SELECT Precio, Existencias FROM Productos WHERE IdProducto = @producto FOR UPDATE", connection, transaction))
            {
                stockCommand.Parameters.AddWithValue("@producto", productId);
                using var reader = stockCommand.ExecuteReader();
                if (!reader.Read())
                    throw new InvalidOperationException("El producto seleccionado ya no existe.");
                unitPrice = reader.GetDecimal("Precio");
                if (reader.GetInt32("Existencias") < quantity)
                    throw new InvalidOperationException("No hay existencias suficientes para la cantidad solicitada.");
            }

            using (var saleCommand = new MySqlCommand(
                "INSERT INTO Ventas (IdCliente, IdProducto, FechaCompra, Cantidad, PrecioUnitario) VALUES (@cliente, @producto, @fecha, @cantidad, @precio)", connection, transaction))
            {
                saleCommand.Parameters.AddWithValue("@cliente", clientId);
                saleCommand.Parameters.AddWithValue("@producto", productId);
                saleCommand.Parameters.AddWithValue("@fecha", purchaseDate);
                saleCommand.Parameters.AddWithValue("@cantidad", quantity);
                saleCommand.Parameters.AddWithValue("@precio", unitPrice);
                saleCommand.ExecuteNonQuery();
            }

            using (var stockCommand = new MySqlCommand(
                "UPDATE Productos SET Existencias = Existencias - @cantidad WHERE IdProducto = @producto", connection, transaction))
            {
                stockCommand.Parameters.AddWithValue("@cantidad", quantity);
                stockCommand.Parameters.AddWithValue("@producto", productId);
                stockCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        private void DeleteSale(int saleId)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            int productId;
            int quantity;
            using (var saleCommand = new MySqlCommand(
                "SELECT IdProducto, Cantidad FROM Ventas WHERE IdVenta = @venta FOR UPDATE", connection, transaction))
            {
                saleCommand.Parameters.AddWithValue("@venta", saleId);
                using var reader = saleCommand.ExecuteReader();
                if (!reader.Read())
                {
                    transaction.Commit();
                    return;
                }
                productId = reader.GetInt32("IdProducto");
                quantity = reader.GetInt32("Cantidad");
            }

            using (var stockCommand = new MySqlCommand(
                "UPDATE Productos SET Existencias = Existencias + @cantidad WHERE IdProducto = @producto", connection, transaction))
            {
                stockCommand.Parameters.AddWithValue("@cantidad", quantity);
                stockCommand.Parameters.AddWithValue("@producto", productId);
                stockCommand.ExecuteNonQuery();
            }

            using (var deleteCommand = new MySqlCommand(
                "DELETE FROM Ventas WHERE IdVenta = @venta", connection, transaction))
            {
                deleteCommand.Parameters.AddWithValue("@venta", saleId);
                deleteCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        private DataTable Query(string sql)
        {
            using var connection = new MySqlConnection(ConnectionString);
            using var command = new MySqlCommand(sql, connection);
            using var adapter = new MySqlDataAdapter(command);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        private int Execute(string sql, params (string Name, object? Value)[] parameters)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = new MySqlCommand(sql, connection, transaction);
            foreach (var (name, value) in parameters)
                command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            var affected = command.ExecuteNonQuery();
            transaction.Commit();
            return affected;
        }

        private void RunAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        private void ShowError(Exception exception) => MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        private static object? DbValue(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static DataGridView CreateGrid() => new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };

        private static TableLayoutPanel CreateTabLayout()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return panel;
        }

        private static Control LabeledControl(string label, Control control)
        {
            var panel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(4, 2, 8, 6) };
            panel.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 0, 0, 2) });
            panel.Controls.Add(control);
            return panel;
        }

        private Button CreateButton(string text, EventHandler handler)
        {
            var button = new Button { Text = text, AutoSize = true, Height = 34, Margin = new Padding(4, 20, 4, 4) };
            button.Click += (_, _) => RunAction(() => handler(button, EventArgs.Empty));
            return button;
        }
    }
}
