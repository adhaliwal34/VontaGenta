\getenv app_pw VONTA_APP_USER_PASSWORD

CREATE USER vonta_genta_app_user WITH PASSWORD :'app_pw';
GRANT CONNECT ON DATABASE vonta_genta_db TO vonta_genta_app_user;

\c vonta_genta_db
GRANT USAGE ON SCHEMA public TO vonta_genta_app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO vonta_genta_app_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO vonta_genta_app_user;
