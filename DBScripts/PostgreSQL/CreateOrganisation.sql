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


/*
INSERT INTO hmsmaster.location_master (channel_id, sub_channel_id, orgid, location_code, location_desc, is_active, created_by, created_date)
 VALUES 
(:p_Channel, :p_SubChannel,:p_orgid, 'HO', 'Head Office', true, :p_CreatedBy, now()),
(:p_Channel, :p_SubChannel,:p_orgid, 'RO', 'Region Office', true, :p_CreatedBy, now()),
(:p_Channel, :p_SubChannel,:p_orgid, 'AR', 'Area Office', true, :p_CreatedBy, now()),
(:p_Channel, :p_SubChannel,:p_orgid, 'BR', 'Branch Office', true, :p_CreatedBy, now());


INSERT INTO hmsmaster.designation_master
(designation_code, designation_name, designation_level, is_active, created_by, created_date, modified_by, modified_date, rowversion, channel_id, orgid, hierarchy_path, code_format, sub_channel_id)
values
('BNHOC', 'Head of Bancassurnace', 1, true, :p_CreatedBy, now(), null, null, 1, :p_Channel, :p_orgid, null, 'BN', :p_SubChannel),
('BNRMGR', 'Regional Manager', 1, true, :p_CreatedBy, now(), null, null, 1, :p_Channel, :p_orgid, null, 'BN', :p_SubChannel),
('BNAMGR', 'Area Manager', 1, true, :p_CreatedBy, now(), null, null, 1, :p_Channel, :p_orgid, null, 'BN', :p_SubChannel),
('BNRLMGR', 'Relation Manager', 1, true, :p_CreatedBy, now(), null, null, 1, :p_Channel, :p_orgid, null, 'BN', :p_SubChannel),
('BNOFF', 'Officers', 1, true, :p_CreatedBy, now(), null, null, 1, :p_Channel, :p_orgid, null, 'BN', :p_SubChannel),
('BNSTAFF', 'Staff', 1, true, :p_CreatedBy, now(), null, null, 1, :p_Channel, :p_orgid, null, 'BN', :p_SubChannel);


update hmsmaster.designation_master
set hierarchy_path = 
case designation_id 
when 21 then '21'
when 22 then '21.22'
when 23 then '21.22.23'
when 24 then '21.22.23.24'
when 25 then '21.22.23.24.25'
when 26 then '21.22.23.24.25.26'
end::ltree
where channel_id = 6 and sub_channel_id = 23;

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

 *
 * */