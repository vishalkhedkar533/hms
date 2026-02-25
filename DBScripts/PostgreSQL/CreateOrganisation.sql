
--execute this at end, to grant administrator access to the new menu items
insert into hms.role_menu_mapping (role_id,menu_id,is_visible,is_enabled,created_by,created_date,orgid)
select 1, mm.menu_id,true,true ,'System', now(), :p_orgid
from hms.menu_master mm
where not exists (select 1 from hms.role_menu_mapping rmm  where mm.menu_id  = rmm.menu_id and rmm.role_id  = 1);

INSERT INTO hmsmaster.keyvalueentries 
    (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES 
    (:p_orgid, 'SR_STATUS', 1, 'Created', NULL, true),
    (:p_orgid, 'SR_STATUS', 2, 'Pending', NULL, true),
    (:p_orgid, 'SR_STATUS', 3, 'Approved', NULL, true),
    (:p_orgid, 'SR_STATUS', 4, 'Rejected', NULL, true);

INSERT INTO hmsmaster.keyvalueentries 
    (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES 
    (:p_orgid, 'APPROVER_DECISION', 1, 'Pending', NULL, true),
    (:p_orgid, 'APPROVER_DECISION', 2, 'Approved', NULL, true),
    (:p_orgid, 'APPROVER_DECISION', 3, 'Rejected', NULL, true),
    (:p_orgid, 'APPROVER_DECISION', 4, 'OnHold', NULL, true);