INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'PREMIUM_COLLECTED_TYPE', 1, 'New Business', null, true);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'PREMIUM_COLLECTED_TYPE', 2, 'Additional premium (NB)', null, true);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'PREMIUM_COLLECTED_TYPE', 3, 'Renewal Premium', null, true);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'PREMIUM_COLLECTED_TYPE', 4, 'Top Up', null, true);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'PREMIUM_COLLECTED_TYPE', 5, 'Premium Reversal', null, true);

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'BankAccType', 'hmsmaster','keyvalueentries',' AND entrycategory = ''BANK_ACC_TYP''');

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'AgentClass', 'hmsmaster','keyvalueentries',' AND entrycategory = ''AGENT_CLASS''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'SalesSubChannels', 'hmsmaster','keyvalueentries',' AND entrycategory = ''SUB_CHANNEL''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'State', 'hmsmaster','keyvalueentries',' AND entrycategory = ''STATE_NAME''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'Occupations', 'hmsmaster','keyvalueentries',' AND entrycategory = ''OCCUPATION''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'MaritalStatus', 'hmsmaster','keyvalueentries',' AND entrycategory = ''MARITAL_STATUS''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'Gender', 'hmsmaster','keyvalueentries',' AND entrycategory = ''GENDER''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'EducationQualification', 'hmsmaster','keyvalueentries',' AND entrycategory = ''EDUCATION_CODE''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'Country', 'hmsmaster','keyvalueentries',' AND entrycategory = ''COUNTRY''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'SalesChannels', 'hmsmaster','keyvalueentries',' AND entrycategory = ''CHANNEL_NAME''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'AgentTypeCategory', 'hmsmaster','keyvalueentries',' AND entrycategory = ''AGENT_TYPE_CAT''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'Salutation', 'hmsmaster','keyvalueentries',' AND entrycategory = ''TITLE''');

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'CandidateType', 'hmsmaster','keyvalueentries',' AND entrycategory = ''CANDIDATE_TYP''');

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'CANDIDATE_TYP', 1, 'Senior', null, false);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'CANDIDATE_TYP', 2, 'Junior', null, false);

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'AgentType', 'hmsmaster','keyvalueentries',' AND entrycategory = ''AGNT_TYP''');

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'AGNT_TYP', 1, 'Type 1', null, false);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'AGNT_TYP', 2, 'Type 2', null, false);

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'CommissionClass', 'hmsmaster','keyvalueentries',' AND entrycategory = ''COMMISSION_CLASS''');

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'COMMISSION_CLASS', 1, 'Class 1', null, false);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(:p_orgid, 'COMMISSION_CLASS', 2, 'Class 2', null, false);

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (:p_orgid, 'AgentProfileMst', 'hmsmaster','keyvalueentries','  AND entrycategory IN( ''AGENT_CLASS'',''SUB_CHANNEL'',''BANK_ACC_TYP'',''STATE_NAME'',''OCCUPATION'',''MARITAL_STATUS'',''GENDER'',''EDUCATION_CODE'',''COUNTRY'',''CHANNEL_NAME'',''AGENT_TYPE_CAT'',''TITLE'',''CANDIDATE_TYP'',''AGNT_TYP'',''COMMISSION_CLASS'')');
INSERT INTO hmsmaster.keyvalueentries
(orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES
-- LICENSE TYPE
(:p_orgid, 'LICENSE_TYPE', 1, 'Permanent',        0, true),
(:p_orgid, 'LICENSE_TYPE', 2, 'Temporary',        0, true),
(:p_orgid, 'LICENSE_TYPE', 3, 'Provisional',      0, true),
(:p_orgid, 'LICENSE_TYPE', 4, 'Contract',         0, true),

-- LICENSE STATUS
(:p_orgid, 'LICENSE_STATUS', 1, 'Active',         0, true),
(:p_orgid, 'LICENSE_STATUS', 2, 'Inactive',       0, true),
(:p_orgid, 'LICENSE_STATUS', 3, 'Expired',        0, true),
(:p_orgid, 'LICENSE_STATUS', 4, 'Suspended',      0, true),

-- VERTICAL
(:p_orgid, 'VERTICAL', 1, 'Healthcare',            0, true),
(:p_orgid, 'VERTICAL', 2, 'Pharmaceutical',        0, true),
(:p_orgid, 'VERTICAL', 3, 'Medical Devices',       0, true),
(:p_orgid, 'VERTICAL', 4, 'Wellness',               0, true),

-- TRAINING GROUP
(:p_orgid, 'TRAINING_GROUP', 1, 'General',          0, true),
(:p_orgid, 'TRAINING_GROUP', 2, 'Advanced',         0, true),
(:p_orgid, 'TRAINING_GROUP', 3, 'Compliance',       0, true),
(:p_orgid, 'TRAINING_GROUP', 4, 'Technical',        0, true);


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

INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, filtercriteria)
VALUES (:p_orgid, 'Channel', 'hmsmaster', 'channel_master', ' AND channel_id AS entryIdentity, channel_name AS entryDesc, channel_code AS entryCategory, is_active AS activeStatus, orgid');

INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, filtercriteria)
VALUES (:p_orgid, 'SubChannel', 'hmsmaster', 'subchannel_master', ' AND sub_channel_id AS entryIdentity, subchannel_name AS entryDesc, subchannel_code AS entryCategory, channel_id AS entryParentId, is_active AS activeStatus, orgid');

INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, filtercriteria)
VALUES (:p_orgid, 'Designation', 'hmsmaster', 'designation_master', ' AND designation_id AS entryIdentity, designation_name AS entryDesc, channel_code AS entryCategory, channel_id AS entryParentId, is_active AS activeStatus');

INSERT INTO hmsmaster.mastertables (
    orgid, 
    entrycategory, 
    schemaname, 
    tablename, 
    filtercriteria, 
    columnalias
) 
VALUES (
    :p_orgid, 
    'CommissionMsts', 
    'hmsmaster', 
    'keyvalueentries', 
    ' AND k.entrycategory IN (''STATE_NAME'', ''GENDER'')', 
    NULL
);
INSERT INTO hmsmaster.keyvalueentries 
(orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES 
(:p_orgid, 'FILE_TYPE', 1, 'Agent CreationManager', NULL, true),
(:p_orgid, 'FILE_TYPE', 2, 'UpdateStatus', NULL, true),
(:p_orgid, 'FILE_TYPE', 3, 'UpdateLocation', NULL, true),
(:p_orgid, 'FILE_TYPE', 4, 'UpdateDesignation', NULL, true),
(:p_orgid, 'FILE_TYPE', 5, 'UpdatePolicy', NULL, true),
(:p_orgid, 'FILE_TYPE', 6, 'UploadCommision', NULL, true),
(:p_orgid, 'FILE_TYPE', 7, 'HoldCommision', NULL, true),
(:p_orgid, 'FILE_TYPE', 8, 'RejectCodeMovement', NULL, true);

INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, filtercriteria)
VALUES (:p_orgid, 'FileType', 'hmsmaster', 'keyvalueentries',' AND entrycategory = ''FILE_TYPE''');

INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, columnalias)
VALUES (:p_orgid, 'Branch', 'hmsmaster', 'branch_master', 'branch_id  AS entryIdentity,  branch_name AS entryDesc, branch_code AS entryCategory, location_master_id AS entryParentId, is_active AS activeStatus, orgid');

INSERT INTO hmsmaster.keyvalueentries 
    (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES 
    (:p_orgid, 'BackgroundJobUserID', 1, 'SchedulerID', NULL, true),
    (:p_orgid, 'BackgroundJobUserPassword', 2, 'SchedulerPassword', NULL, true);

--execute this at end, to grant administrator access to the new menu items
INSERT INTO hms.roles
(role_name, description, is_system_role, is_active, created_by, created_date, modified_by, modified_date, rowversion, orgid)
VALUES('admin', 'administrator', true, true, null, null, null, null, 1, :p_orgid);

insert into hms.role_menu_mapping (role_id,menu_id,is_visible,is_enabled,created_by,created_date,orgid)
select :p_AdminRoleId, mm.menu_id,true,true ,'System', now(), :p_orgid
from hms.menu_master mm
where not exists (select 1 from hms.role_menu_mapping rmm  where mm.menu_id  = rmm.menu_id and rmm.role_id  = :p_AdminRoleId);

INSERT INTO hms.hms_dashboard (orgid, channel_id, subchannel_id, totalentitiescount, totalentitiesthismonth, entitiescreatedthismonth, entitiescreatedprevmonth, entitiesterminatedthismonth, entitiesterminatedprevmonth, entitiesnetthismonth, licenseexpiringin30months, certificateexpiringin30months, mbgcriterianotmet) 
VALUES (:p_orgid, null, null, 150.0, 12.0, 8.0, 5.0, 2.0, 1.0, 6.0, 3.0, 4.0, 1.0);

INSERT INTO hms."user"
(username, email_id, mobile_number, "password", is_active, is_locked, last_login_date, created_by, created_date, modified_by, modified_date, rowversion, password_changed_date, "LockoutEndTime", failedloginattempts, lockoutendtime, orgid, reporting_mgr, refreshtoken, refreshtokenexpirytime)
VALUES('systemadmin', 'systemadmin@dummy.com', '9999999999', '$2a$11$TWfvFQUKtWysi2cBD0I2MOrJAQAQTiX.S4b2.WzlUVivbR4KrexFa', true, false, now(), 12, now(), null, null, 1, null, null, 0, null, :p_orgid, null, null, null);

INSERT INTO hms.user_role_mapping
(user_id, role_id, is_primary, effective_from, effective_to, is_active, created_by, created_date, modified_by, modified_date, rowversion, orgid)
select u.user_id, r.role_id, true, CURRENT_DATE, CURRENT_DATE + interval '1 year' , true ,  u.user_id, now(), null, null, 1, :p_orgid
from hms."user" u join hms.roles r on u.orgid = r.orgid and r.role_id = :p_AdminRoleId and u.user_id = :p_UserId
where u.orgid = :p_orgid and not exists (select 1 from hms.user_role_mapping urm where urm.orgid = :p_orgid and u.user_id = urm.user_id  and r.role_id = urm.role_id );
