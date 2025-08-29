--Dummy text
insert into hms."api_config"(config_key,config_value) values ('wrong_attempts_allowed', '5');

insert into hms.errorMaster (error_id,area,error_msg) values (1001, 'LoginConstants', 'Invalid Credentials' );
insert into hms.errorMaster (error_id,area,error_msg) values (1002, 'LoginConstants', 'Account is locked. Try again after {0}' );
insert into hms.errorMaster (error_id,area,error_msg) values (1003, 'LoginConstants', 'User has no active primary role.' );
insert into hms.errorMaster (error_id,area,error_msg) values (1101, 'Common', 'Success' );


INSERT INTO applogs.applog_filter_policy (minimum_level, excluded_categories)
VALUES ('Information', ARRAY['Microsoft', 'System.Net.Http', 'Microsoft.EntityFrameworkCore.Database.Command']);
