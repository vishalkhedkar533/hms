--Dummy text
insert into hms."api_config"(config_key,config_value) values ('wrong_attempts_allowed', '5');

insert into hms.errorMaster (error_id,area,error_msg) values (1001, 'LoginConstants', 'Invalid Credentials' );
insert into hms.errorMaster (error_id,area,error_msg) values (1002, 'LoginConstants', 'Account is locked. Try again after {0}' );
insert into hms.errorMaster (error_id,area,error_msg) values (1003, 'LoginConstants', 'User has no active primary role.' );
insert into hms.errorMaster (error_id,area,error_msg) values (1101, 'Common', 'Success' );


INSERT INTO applogs.applog_filter_policy (minimum_level, excluded_categories)
VALUES ('Information', ARRAY['Microsoft', 'System.Net.Http', 'Microsoft.EntityFrameworkCore.Database.Command']);

insert into hms.errorMaster (error_id,area,error_msg) values (1201, 'AgentConstants', 'Agent Not Found.' );


INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'BankAccType', 'hmsmaster','keyvalueentries',' AND entrycategory = ''BANK_ACC_TYP''');

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'AgentClass', 'hmsmaster','keyvalueentries',' AND entrycategory = ''AGENT_CLASS''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'SalesSubChannels', 'hmsmaster','keyvalueentries',' AND entrycategory = ''SUB_CHANNEL''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'State', 'hmsmaster','keyvalueentries',' AND entrycategory = ''STATE_NAME''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'Occupations', 'hmsmaster','keyvalueentries',' AND entrycategory = ''OCCUPATION''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'MaritalStatus', 'hmsmaster','keyvalueentries',' AND entrycategory = ''MARITAL_STATUS''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'Gender', 'hmsmaster','keyvalueentries',' AND entrycategory = ''GENDER''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'EducationQualification', 'hmsmaster','keyvalueentries',' AND entrycategory = ''EDUCATION_CODE''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'Country', 'hmsmaster','keyvalueentries',' AND entrycategory = ''COUNTRY''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'SalesChannels', 'hmsmaster','keyvalueentries',' AND entrycategory = ''CHANNEL_NAME''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'AgentTypeCategory', 'hmsmaster','keyvalueentries',' AND entrycategory = ''AGENT_TYPE_CAT''') ;
INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'Salutation', 'hmsmaster','keyvalueentries',' AND entrycategory = ''TITLE''');

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'CandidateType', 'hmsmaster','keyvalueentries',' AND entrycategory = ''CANDIDATE_TYP''');

insert into hms."api_config"(config_key,config_value) values ('agent_create_chunk_size', '5');

insert into hms.errorMaster (error_id,area,error_msg) values (1201, 'MasterConstants', 'Master Entry Not Found.' );

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'CANDIDATE_TYP', 1, 'Senior', null, false);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'CANDIDATE_TYP', 2, 'Junior', null, false);

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'AgentType', 'hmsmaster','keyvalueentries',' AND entrycategory = ''AGNT_TYP''');

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'AGNT_TYP', 1, 'Type 1', null, false);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'AGNT_TYP', 2, 'Type 2', null, false);

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'CommissionClass', 'hmsmaster','keyvalueentries',' AND entrycategory = ''COMMISSION_CLASS''');

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'COMMISSION_CLASS', 1, 'Class 1', null, false);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'COMMISSION_CLASS', 2, 'Class 2', null, false);

INSERT INTO hmsmaster.mastertables(orgid,entrycategory,schemaname,tablename,filtercriteria)
values (2, 'AgentProfileMst', 'hmsmaster','keyvalueentries','  AND entrycategory IN( ''AGENT_CLASS'',''SUB_CHANNEL'',''BANK_ACC_TYP'',''STATE_NAME'',''OCCUPATION'',''MARITAL_STATUS'',''GENDER'',''EDUCATION_CODE'',''COUNTRY'',''CHANNEL_NAME'',''AGENT_TYPE_CAT'',''TITLE'',''CANDIDATE_TYP'',''AGNT_TYP'',''COMMISSION_CLASS'')');

INSERT INTO hms.roles (role_id, role_name, description, is_system_role, is_active, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(2, 'Commission Config Creator', 'This role will allow the user to create commission cofiguration', true, true, '', CURRENT_DATE, '', CURRENT_DATE, 0);

INSERT INTO hms.roles (role_id, role_name, description, is_system_role, is_active, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(3, 'Commission Config Viewer', 'This role will allow the user to view commission cofiguration', true, true, '', CURRENT_DATE, '', CURRENT_DATE, 0);

INSERT INTO hms.roles (role_id, role_name, description, is_system_role, is_active, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(4, 'Commission Config Approver', 'This role will allow the user to approve commission cofiguration', true, true, '', CURRENT_DATE, '', CURRENT_DATE, 0);

INSERT INTO hms.menu_master(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1003, 'Save Commission Configuration', null, '', 0, false, false, '', CURRENT_DATE, '', CURRENT_DATE, 0);

INSERT INTO hms.menu_master(menu_id, menu_name, parent_menu_id, route_path, display_order, is_active, is_internal, created_by, created_date, modified_by, modified_date, rowversion)
VALUES(1004, 'View Commission Configuration', null, '', 0, false, false, '', CURRENT_DATE, '', CURRENT_DATE, 0);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'PREMIUM_COLLECTED_TYPE', 1, 'New Business', null, true);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'PREMIUM_COLLECTED_TYPE', 2, 'Additional premium (NB)', null, true);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'PREMIUM_COLLECTED_TYPE', 3, 'Renewal Premium', null, true);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'PREMIUM_COLLECTED_TYPE', 4, 'Top Up', null, true);

INSERT INTO hmsmaster.keyvalueentries (orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES(2, 'PREMIUM_COLLECTED_TYPE', 5, 'Premium Reversal', null, true);

insert into hms.errorMaster (error_id,area,error_msg) values (1401, 'CommissionConstants', 'Commission Config Not Found.' );

insert into hms.errorMaster (error_id,area,error_msg) values (1501, 'JobConstants', 'Job Not Found.' );

INSERT INTO hmsmaster.keyvalueentries
(orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES
-- LICENSE TYPE
(2, 'LICENSE_TYPE', 1, 'Permanent',        0, true),
(2, 'LICENSE_TYPE', 2, 'Temporary',        0, true),
(2, 'LICENSE_TYPE', 3, 'Provisional',      0, true),
(2, 'LICENSE_TYPE', 4, 'Contract',         0, true),

-- LICENSE STATUS
(2, 'LICENSE_STATUS', 1, 'Active',         0, true),
(2, 'LICENSE_STATUS', 2, 'Inactive',       0, true),
(2, 'LICENSE_STATUS', 3, 'Expired',        0, true),
(2, 'LICENSE_STATUS', 4, 'Suspended',      0, true),

-- VERTICAL
(2, 'VERTICAL', 1, 'Healthcare',            0, true),
(2, 'VERTICAL', 2, 'Pharmaceutical',        0, true),
(2, 'VERTICAL', 3, 'Medical Devices',       0, true),
(2, 'VERTICAL', 4, 'Wellness',               0, true),

-- TRAINING GROUP
(2, 'TRAINING_GROUP', 1, 'General',          0, true),
(2, 'TRAINING_GROUP', 2, 'Advanced',         0, true),
(2, 'TRAINING_GROUP', 3, 'Compliance',       0, true),
(2, 'TRAINING_GROUP', 4, 'Technical',        0, true);

select * from hmsmaster.keyvalueentries k where k.entrycategory  in('CHANNEL_NAME', 'SUB_CHANNEL','DESIGNATION')


INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, filtercriteria)
VALUES (2, 'Channel', 'hmsmaster', 'channel_master', ' AND channel_id AS entryIdentity, channel_name AS entryDesc, channel_code AS entryCategory, is_active AS activeStatus, orgid');

INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, filtercriteria)
VALUES (2, 'SubChannel', 'hmsmaster', 'subchannel_master', ' AND sub_channel_id AS entryIdentity, subchannel_name AS entryDesc, subchannel_code AS entryCategory, channel_id AS entryParentId, is_active AS activeStatus, orgid');

INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, filtercriteria)
VALUES (2, 'Designation', 'hmsmaster', 'designation_master', ' AND designation_id AS entryIdentity, designation_name AS entryDesc, channel_code AS entryCategory, channel_id AS entryParentId, is_active AS activeStatus');

update hmsmaster.mastertables 
set columnalias = filtercriteria 
where entrycategory in ('Channel','SubChannel','Designation');

update hmsmaster.mastertables 
set filtercriteria  = null
where entrycategory in ('Channel','SubChannel','Designation');

update hmsmaster.mastertables 
set columnalias = 'channel_id AS entryIdentity, channel_name AS entryDesc, channel_code AS entryCategory, is_active AS activeStatus, orgid' 
where entrycategory in ('Channel');

update hmsmaster.mastertables 
set columnalias = 'sub_channel_id AS entryIdentity, subchannel_name AS entryDesc, subchannel_code AS entryCategory, channel_id AS entryParentId, is_active AS activeStatus, orgid' 
where entrycategory in ('SubChannel');

update hmsmaster.mastertables 
set columnalias = 'designation_id AS entryIdentity, designation_name AS entryDesc, channel_code AS entryCategory, channel_id AS entryParentId, is_active AS activeStatus' 
where entrycategory in ('Designation');

--delete from hmsmaster.mastertables where orgid = 2 and entrycategory = 'CommissionMsts'

INSERT INTO hmsmaster.mastertables (
    orgid, 
    entrycategory, 
    schemaname, 
    tablename, 
    filtercriteria, 
    columnalias
) 
VALUES (
    2, 
    'CommissionMsts', 
    'hmsmaster', 
    'keyvalueentries', 
    ' AND k.entrycategory IN (''STATE_NAME'', ''GENDER'')', 
    NULL
);
INSERT INTO hmsmaster.keyvalueentries 
(orgid, entrycategory, entryidentity, entrydesc, entryparentid, activestatus)
VALUES 
(2, 'FILE_TYPE', 1, 'Agent CreationManager', NULL, true),
(2, 'FILE_TYPE', 2, 'UpdateStatus', NULL, true),
(2, 'FILE_TYPE', 3, 'UpdateLocation', NULL, true),
(2, 'FILE_TYPE', 4, 'UpdateDesignation', NULL, true),
(2, 'FILE_TYPE', 5, 'UpdatePolicy', NULL, true),
(2, 'FILE_TYPE', 6, 'UploadCommision', NULL, true),
(2, 'FILE_TYPE', 7, 'HoldCommision', NULL, true),
(2, 'FILE_TYPE', 8, 'RejectCodeMovement', NULL, true);

INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, filtercriteria)
VALUES (2, 'FileType', 'hmsmaster', 'keyvalueentries',' AND entrycategory = ''FILE_TYPE''');



INSERT INTO hmsmaster.location_master (channel_id, sub_channel_id, orgid, location_code, 
location_desc, is_active, created_by, created_date)
 VALUES 
(1, 1,2, 'HO', 'Head Office', true, 'System', '2026-02-06 10:00:00'),
(1, 1,2, 'ZO', 'Zonal Office', true, 'System', '2026-02-06 10:00:00'),
(1, 1,2, 'RO', 'Regional Office', true, 'System', '2026-02-06 10:00:00'),
(1, 1,2, 'AR', 'Area Office', true, 'System', '2026-02-06 10:00:00'),
(1, 1,2, 'UN', 'Unit Office', true, 'System', '2026-02-06 10:00:00');

INSERT INTO hmsmaster.branch_master
(orgid, branch_code, branch_name, address, state, phone_number, email_id, is_active, location_master_id, created_by, created_date)
VALUES(2, 'Agency-HO', 'Agency Head Office', 'Agency Head Office Address', 1, '', '', true, 1, 'system', now());

INSERT INTO hmsmaster.branch_master
(orgid, branch_code, branch_name, address, state, phone_number, email_id, is_active, location_master_id, created_by, created_date)
VALUES
(2, 'WESTZO', 'Agency West Zone Office', 'Agency West Zone Address', 1, '', '', true, 2, 'system', now()),
(2, 'NORTHZO', 'Agency West Zone Office', 'Agency North Zone Address', 1, '', '', true, 2, 'system', now()),
(2, 'SOUTHZO', 'Agency South Zone Office', 'Agency South Zone Address', 1, '', '', true, 2, 'system', now()),
(2, 'EASTZO', 'Agency East Zone Office', 'Agency East Zone Address', 1, '', '', true, 2, 'system', now());

INSERT INTO hmsmaster.branch_master
(orgid, branch_code, branch_name, address, state, phone_number, email_id, is_active, location_master_id, created_by, created_date)
VALUES
(2, 'MAHAREG', 'Maharastra Region Office', 'Maharastra Region Address', 1, '', '', true, 3, 'system', now()),
(2, 'GUJREG', 'Gujarat Region Office', 'Gujarat Region Address', 1, '', '', true, 3, 'system', now()),
(2, 'RAJAREG', 'Rajastan Region Office', 'Rajastan Region Address', 1, '', '', true, 3, 'system', now());


INSERT INTO hmsmaster.branch_master
(orgid, branch_code, branch_name, address, state, phone_number, email_id, is_active, location_master_id, created_by, created_date)
VALUES
(2, 'MUMAREA', 'Mumbai Area Office', 'Mumbai Area Address', 1, '', '', true, 4, 'system', now()),
(2, 'BEEDAREA', 'Beed Area Office', 'Beed Area Address', 1, '', '', true, 4, 'system', now()),
(2, 'PUNEAREA', 'Pune Area Office', 'Pune Area Address', 1, '', '', true, 4, 'system', now());


INSERT INTO hmsmaster.branch_master
(orgid, branch_code, branch_name, address, state, phone_number, email_id, is_active, location_master_id, created_by, created_date)
VALUES
(2, 'THANEUNT', 'Thane Unit Office', 'Thane Unit Address', 1, '', '', true, 5, 'system', now()),
(2, 'ANDHERIUNIT', 'Andheri Unit Office', 'Andheri Unit Address', 1, '', '', true, 5, 'system', now()),
(2, 'COLABAUNIT', 'Colaba unit Office', 'Colaba Unit Address', 1, '', '', true, 5, 'system', now()),
(2, 'SHIRURUNIT', 'Shirur unit Office', 'Shirur Unit Address', 1, '', '', true, 5, 'system', now()),
(2, 'KATRAJUNIT', 'Katraj unit Office', 'Katraj Unit Address', 1, '', '', true, 5, 'system', now());


INSERT INTO hmsmaster.channel_branch_heirarchy
(orgid, channel_id, sub_channel_id, hierarchy_path, created_by, created_date, modified_by, modified_date, effective_from_date, effective_to_date)
VALUES(2, 1, 1, '6', 'system', now(), 'system', now(), now(), now());

INSERT INTO hmsmaster.channel_branch_heirarchy
(orgid, channel_id, sub_channel_id, hierarchy_path, created_by, created_date, modified_by, modified_date, effective_from_date, effective_to_date)
VALUES(2, 1, 1, '6.7', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.8', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.9', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.10', 'system', now(), 'system', now(), now(), now());

INSERT INTO hmsmaster.channel_branch_heirarchy
(orgid, channel_id, sub_channel_id, hierarchy_path, created_by, created_date, modified_by, modified_date, effective_from_date, effective_to_date)
values
(2, 1, 1, '6.7.11', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.7.12', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.7.13', 'system', now(), 'system', now(), now(), now());


INSERT INTO hmsmaster.channel_branch_heirarchy
(orgid, channel_id, sub_channel_id, hierarchy_path, created_by, created_date, modified_by, modified_date, effective_from_date, effective_to_date)
values
(2, 1, 1, '6.7.11.17', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.7.11.18', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.7.11.19', 'system', now(), 'system', now(), now(), now());


INSERT INTO hmsmaster.channel_branch_heirarchy
(orgid, channel_id, sub_channel_id, hierarchy_path, created_by, created_date, modified_by, modified_date, effective_from_date, effective_to_date)
values
(2, 1, 1, '6.7.11.17.20', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.7.11.17.21', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.7.11.17.22', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.7.11.18.23', 'system', now(), 'system', now(), now(), now()),
(2, 1, 1, '6.7.11.19.24', 'system', now(), 'system', now(), now(), now());


INSERT INTO hmsmaster.mastertables (orgid, entrycategory, schemaname, tablename, columnalias)
VALUES (2, 'Branch', 'hmsmaster', 'branch_master', 'branch_id  AS entryIdentity,  branch_name AS entryDesc, branch_code AS entryCategory, location_master_id AS entryParentId, is_active AS activeStatus, orgid');


select * from hmsmaster.uicontrol_master

insert into hmsmaster.uicontrol_master(uicontrolmenu_id,ui_object_name,ui_object_type)
values
(1,'Agent','TAB'),
(2,'Channel','TextBox'),
(3,'SubChannel','TextBox'),
(4,'Location','TextBox'),
(5,'Designation','TextBox');

insert into hmsmaster.uicontrol_hierarchy(uicontrolmenu_id,hierarchy_path)
values
(1,'1'::ltree),
(2,'1.2'::ltree),
(3,'1.3'::ltree),
(4,'1.4'::ltree),
(5,'1.5'::ltree);
insert into hmsmaster.uicontrol_master(uicontrolmenu_id,ui_object_name,ui_object_type)
values
(6,'PersonalInfo','Section'),
(7,'MobileNo','TextBox'),
(8,'Home','TextBox'),
(9,'Work','TextBox'),
(10,'ContactPerName','TextBox'),
(11,'ContactPerPhone','TextBox'),
(12,'ContactPerEmailID','TextBox'),
(13,'ContactPerDesig','TextBox');

insert into hmsmaster.uicontrol_hierarchy(uicontrolmenu_id,hierarchy_path)
values
(6,'1.6'::ltree),
(7,'1.6.7'::ltree),
(8,'1.6.8'::ltree),
(9,'1.6.9'::ltree),
(10,'1.6.10'::ltree),
(11,'1.6.11'::ltree),
(12,'1.6.12'::ltree),
(13,'1.6.13'::ltree);

INSERT INTO hmsmaster.org_uicontrol
(orgid, hierarchy_id, role_id, allow_edit, render_control, access_granted_on, access_granted_by)
select 2,uch.hierarchy_id , 1, true,true,now(),1
from hmsmaster.uicontrol_hierarchy uch
where not exists(select 1 from hmsmaster.org_uicontrol ouc where uch.hierarchy_id  = ouc.hierarchy_id )

-- 1. Insert UI Components (Hierarchy)
INSERT INTO hmsmaster.ui_components (component_id, path, label, elementType) VALUES 
(100, 'Agent', 'Agent', 'Screen'),
(101, 'Agent.Personal', 'Personal', 'Tab'),
(102, 'Agent.Personal.PersonalInfo', 'PersonalInfo', 'Section'),
(103, 'Agent.Personal.IndividualAction', 'IndividualAction', 'Section');

-- 2. Insert UI Fields (Controls)
-- Fields for PersonalInfo (component_id 102)
INSERT INTO hmsmaster.ui_fields (cntrl_id, component_id, cntrl_name) VALUES 
(501, 102, 'title'),
(502, 102, 'lastName');

-- Fields for IndividualAction (component_id 103)
INSERT INTO hmsmaster.ui_fields (cntrl_id, component_id, cntrl_name) VALUES 
(503, 103, 'Channel'),
(504, 103, 'subChannel');
