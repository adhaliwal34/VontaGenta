BEGIN;

SET client_encoding = 'UTF8';

INSERT INTO appactions (id, name) VALUES
(0, 'None'),
(1, 'Create'),
(2, 'Edit'),
(4, 'View'),
(8, 'Delete');

INSERT INTO aspnetroles (id, name, normalizedname, concurrencystamp)
VALUES (gen_random_uuid()::text, 'Administrator', 'ADMINISTRATOR', gen_random_uuid()::text);

INSERT INTO currencies (name, code, symbol) VALUES
('Australian dollar','AUD','$'),
('Canadian dollar','CAD','$'),
('Renminbi','CNY','¥'),
('Egyptian pound','EGP','LE'),
('Hong Kong dollar','HKD','$'),
('Icelandic króna','ISK','kr'),
('Indonesian rupiah','IDR','Rp'),
('Japanese yen','JPY','¥'),
('Malaysian ringgit','MYR','RM'),
('Mauritian rupee','MUR','Rs'),
('Indian rupee','INR','₹'),
('New Zealand dollar','NZD','$'),
('Pakistani rupee','PKR','Rs'),
('Philippine peso','PHP','₱'),
('Singapore dollar','SGD','$'),
('New Taiwan dollar','TWD','$'),
('Thai baht','THB','฿'),
('Sterling','GBP','£'),
('United States dollar','USD','$');

INSERT INTO apptimezones (zoneid, friendlyname) VALUES
('Etc/UTC','UTC'),
('Europe/London','UK, Guernsey, Isle of Man, Jersey, Northern Ireland'),
('Europe/Dublin','Ireland'),
('Asia/Singapore','Singapore'),
('Asia/Kuala_Lumpur','Malaysia');

COMMIT;