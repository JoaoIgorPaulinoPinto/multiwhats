import mysql from "mysql2/promise";

// const db = mysql.createPool({
//     host: process.env.DB_HOST || "localhost",
//     port: Number(process.env.DB_PORT) || 3306,
//     user: process.env.DB_USER || "root",
//     password: process.env.DB_PASSWORD || "",
//     database: process.env.DB_DATABASE || "",
//     waitForConnections: true,
//     connectionLimit: 5
// });
const db = mysql.createPool({
    host: "localhost",
    port: 3306,
    user: "root",
    password:"12345678",
    database: "test",
    waitForConnections: true,
    connectionLimit: 5
});
export default db;
