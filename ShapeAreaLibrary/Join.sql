SELECT T1.ProductName, T2.CategoryName, T1.Price FROM Products T1 
LEFT JOIN Categories T2 ON T1.CategoryId = T2.CategoryId ORDER BY T1.CategoryId