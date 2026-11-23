\getenv owner_pw VONTA_DB_USER_PASSWORD

CREATE DATABASE vonta_genta_db WITH ENCODING 'UTF8';
CREATE USER vonta_genta_db_user WITH PASSWORD :'owner_pw';
GRANT ALL PRIVILEGES ON DATABASE vonta_genta_db TO vonta_genta_db_user;

\c vonta_genta_db
GRANT ALL ON SCHEMA public TO vonta_genta_db_user;
