# Sistema de Ventas de Tienda

Aplicación de escritorio en C# con Windows Forms y MySQL. Permite insertar, seleccionar (consultar) y eliminar clientes, productos y ventas. Cada venta guarda el cliente, el producto, la fecha, la cantidad y el precio unitario del momento de la compra. El inventario se descuenta al registrar la venta.

## Requisitos

- Windows y .NET 10 SDK
- MySQL Server (por ejemplo, configurado con MySQL Workbench)
- MySQL Workbench para ejecutar el script inicial

## Preparar la base de datos

1. Abra `database.sql` en MySQL Workbench.
2. Conéctese a su servidor MySQL y ejecute el script completo. Este crea `tienda_ventas` y las tablas `Clientes`, `Productos` y `Ventas`.
3. Abra `pruebaaa.slnx` en Visual Studio y ejecute el proyecto. NuGet restaurará `MySqlConnector` automáticamente.
4. En la parte superior de la aplicación, introduzca servidor, puerto, base de datos, usuario y contraseña; pulse **Conectar / actualizar**.

La configuración de conexión se introduce en la aplicación y no se guarda en el código fuente.

## Uso

- **Clientes:** registre nombre, teléfono y correo. Seleccione una fila y use **Eliminar seleccionado** para borrar el registro.
- **Productos:** registre nombre, precio y existencias iniciales. El precio y las existencias deben ser cero o mayores.
- **Ventas:** seleccione un cliente, un producto con existencias, fecha y cantidad; pulse **Registrar venta**. El total se calcula en la consulta y las existencias se actualizan automáticamente.
- Las ventas se muestran con el cliente, producto, fecha, cantidad, precio unitario y total. Seleccione una venta y pulse **Eliminar venta** para eliminarla.

Las claves foráneas impiden eliminar clientes o productos que ya tengan ventas asociadas; primero elimine esas ventas.
