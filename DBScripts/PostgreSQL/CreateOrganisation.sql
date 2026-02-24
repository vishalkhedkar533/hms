
--execute this at end, to grant administrator access to the new menu items
insert into hms.role_menu_mapping (role_id,menu_id,is_visible,is_enabled,created_by,created_date,orgid)
select 1, mm.menu_id,true,true ,'System', now(), 2
from hms.menu_master mm
where not exists (select 1 from hms.role_menu_mapping rmm  where mm.menu_id  = rmm.menu_id and rmm.role_id  = 1);
