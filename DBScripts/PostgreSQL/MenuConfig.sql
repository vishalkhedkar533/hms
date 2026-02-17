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


insert into hms.role_menu_mapping (role_id,menu_id,is_visible,is_enabled,created_by,created_date,orgid)
select 1, mm.menu_id,true,true ,'System', now(), 2
from hms.menu_master mm
where not exists (select 1 from hms.role_menu_mapping rmm  where mm.menu_id  = rmm.menu_id and rmm.role_id  = 1);
