-- Base de Datos Sistema de Ventas TiendaDB
CREATE DATABASE IF NOT EXISTS TiendaDB;
USE TiendaDB;

CREATE TABLE Clientes (
    IdCliente INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Telefono VARCHAR(15),
    Direccion VARCHAR(150)
);

CREATE TABLE Productos (
    IdProducto INT AUTO_INCREMENT PRIMARY KEY,
    NombreProducto VARCHAR(100) NOT NULL,
    Precio DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL
);

CREATE TABLE Ventas (
    IdVenta INT AUTO_INCREMENT PRIMARY KEY,
    IdCliente INT NOT NULL,
    IdProducto INT NOT NULL,
    FechaCompra DATE NOT NULL,
    Cantidad INT NOT NULL,
    FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente),
    FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto)
);

INSERT INTO Clientes (Nombre, Telefono, Direccion) VALUES
('Juan Perez','0991111111','Santo Domingo'),
('Maria Lopez','0992222222','Quito'),
('Carlos Ruiz','0993333333','Guayaquil');

INSERT INTO Productos (NombreProducto, Precio, Stock) VALUES
('Laptop HP',850.00,10),
('Mouse Logitech',25.50,50),
('Teclado Redragon',45.00,30),
('Monitor Samsung',220.00,15);

INSERT INTO Ventas (IdCliente, IdProducto, FechaCompra, Cantidad) VALUES
(1,1,CURDATE(),1),
(1,2,CURDATE(),2),
(2,3,CURDATE(),1),
(3,4,CURDATE(),1);
