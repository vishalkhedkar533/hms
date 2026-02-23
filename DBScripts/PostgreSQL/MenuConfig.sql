INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1001, 'Search agent', null, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1002, 'Modify Add agent', null, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1005, 'Access Management', null, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1006, 'Fetct List', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

update hms.menu_master set menu_name = 'Fetch Roles' where menu_id= 1006;

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1007, 'Create Role', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1008, 'Delete Role', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1009, 'Get Menu Access for Role', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1010, 'Get Users Under a Role', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1011, 'Add User Under a Role', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1012, 'Remove User From a Role', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1013, 'Grant Menu Access to Role', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1014, 'Revoke Menu Access to Role', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1015, 'Update UI Access', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1016, 'UI Control Access', 1005, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1017, 'Channel Management', null, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1018, 'Create Update Delete', 1017, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

update hms.menu_master set menu_name = 'Add Remove User from Role' where menu_id = 1011;
delete from hms.role_menu_mapping where menu_id= 1012;
delete from hms.menu_master where menu_id = 1012;
delete from hms.role_menu_mapping where menu_id= 1014;
delete from hms.menu_master where menu_id = 1014;
update hms.menu_master set menu_name = 'Modify Menu Access to Role' where menu_id = 1013;

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1019, 'Manage Channel (designation/location/branch) ', 1017, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1020, 'Service Request', null, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1021, 'Create Agent Update SR', 1020, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);

INSERT INTO hms.menu_master
(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1022, 'Approve Reject Agent Update SR', 1020, null, 1, true, true, 'navin', '2026-02-16', null, null, 0);
