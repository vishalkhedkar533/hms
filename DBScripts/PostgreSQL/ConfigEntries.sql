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

-- 1. Insert UI Components (Hierarchy)
INSERT INTO hmsmaster.ui_components (component_id, path, label, elementType) VALUES 
(	100	,'Agent',	'Agent',	'Screen'),
(	101	,'Agent.Personal',	'Personal',	'Tab'),
(	102	,'Agent.Personal.IndividualAgentAction',	'Individual Agent Action',	'Section'),
(	103	,'Agent.Personal.PersonalDetails',	'Personal Details',	'Section'),
(	104	,'Agent.Personal.ContactInformation',	'Contact Information',	'Section'),
(	105	,'Agent.Personal.EmployeeInformationConfig',	'Employee Information Config',	'Section'),
(	106	,'Agent.Personal.FinancialDetails',	'Financial Details',	'Section'),
(	107	,'Agent.Personal.OtherPersonalDetails',	'Other Personal Details',	'Section'),
(	108	,'Agent.Personal.Address',	'Address',	'Section'),
(	109	,'Agent.PeopleHeirarchy',	'People Heirarchy',	'Tab'),
(	110	,'Agent.GeographicHeirarchy',	'Geographic Heirarchy',	'Tab'),
(	111	,'Agent.PartnersMapped',	'Partners Mapped',	'Tab'),
(	112	,'Agent.AuditLog',	'Audit Log',	'Tab'),
(	113	,'Agent.AuditLog.LogDetails',	'Log Details',	'Section'),
(	127	,'Agent.License',	'License',	'Tab'),
(	128	,'Agent.License.LicenseDetails',	'License Details',	'Section'),
(	129	,'Agent.License.TrainingDetails',	'Training Details',	'Section'),
(	130	,'Agent.License.FinancialDetails',	'Financial Details',	'Section'),
(	131	,'Agent.License.ProductDetails',	'Product Details',	'Section'),
(	132	,'Agent.License.OtherDetails',	'Other Details',	'Section'),
(	133	,'Agent.License.OtherDetails',	'Other Details',	'Section'),
(	134	,'Agent.Financial',	'Financial',	'Tab'),
(	135	,'Agent.Financial.FinancialDetails',	'Financial Details',	'Section'),
(	136	,'Agent.Entity360',	'Entity 360',	'Tab'),
(	137	,'Agent.Training',	'Training',	'Tab'),
(	138	,'Agent.Training.Branch',	'Branch',	'Section'),
(	139	,'Agent.Training.Organisation',	'Organisation',	'Section');

INSERT INTO hmsmaster.ui_fields (cntrl_id, component_id, cntrl_name) VALUES 
(1001,102,'Channel Name'),
(1002,102,'Sub Channel'),
(1003,102,'Location'),
(1004,102,'Designation'),
(1005,103,'Title'),
(1006,103,'First Name'),
(1007,103,'Middle Name'),
(1008,103,'Last Name'),
(1009,103,'Father/Husband Name'),
(1010,103,'Agent  Gender'),
(1011,104,'Mobile No'),
(1012,104,'Home No'),
(1013,104,'Work No'),
(1014,104,'Email ID'),
(1015,104,'Contact Person Name'),
(1016,104,'Contact Person No'),
(1017,104,'Contact Person Email'),
(1018,104,'Contact Person Designation'),
(1019,105,'Agent ID'),
(1020,105,'Application Docket No'),
(1021,105,'Agent Code'),
(1022,105,'Candidate Type'),
(1023,105,'Agent Type'),
(1024,105,'Employee Code'),
(1025,105,'Start Date'),
(1026,105,'Appointment Date'),
(1027,105,'Incorporation Date'),
(1028,105,'Agent Type Category'),
(1029,105,'Agent Classification'),
(1030,105,'CMS Agent Type'),
(1031,106,'PAN Aadhar Linking Flag'),
(1032,106,'Sec 206 AB Flag'),
(1033,106,'Tax Status'),
(1034,106,'Service Tax No'),
(1035,106,'Factoring House'),
(1036,106,'Payee Name'),
(1037,106,'PAN'),
(1038,106,'Bank Name'),
(1039,106,'Bank Branch Name'),
(1040,106,'Bank Account No'),
(1041,106,'Bank Account Type'),
(1042,106,'MICR'),
(1043,106,'IFSC'),
(1044,106,'Payment Mode'),
(1045,107,'Date Of Birth'),
(1046,107,'Marital Status'),
(1047,107,'Education Level'),
(1048,107,'Work Profile'),
(1049,107,'Annual Income'),
(1050,107,'Nominee Name'),
(1051,107,'Nominee Relation'),
(1052,107,'Nominee Age'),
(1053,107,'Occupation'),
(1054,107,'URN'),
(1055,108,'Address Type'),
(1056,108,'Address Line 1'),
(1057,108,'Address Line 2'),
(1058,108,'Address Line 3'),
(1059,108,'City'),
(1060,108,'State'),
(1061,108,'State EID'),
(1062,108,'Country'),
(1063,108,'PIN'),
(1064,109,'Hierarchy Navigator'),
(1065,109,'Employee List'),
(1066,110,'Hierarchy Navigator'),
(1067,110,'Employee List'),
(1068,128,'Contact Person Name'),
(1069,128,'Agency Type Category'),
(1070,128,'Agent Classification'),
(1071,128,'License Status'),
(1072,128,'License Expiry Date'),
(1073,128,'License Issue Date'),
(1074,128,'License Type'),
(1075,128,'License No'),
(1076,128,'Commission Class'),
(1077,129,'Training Group Type'),
(1078,129,'Refresher Training Completed'),
(1079,130,'Channel Name'),
(1080,131,'ULIP Flag'),
(1081,132,'Is Migrated'),
(1082,132,'Main Partner Client Code'),
(1083,133,'Is Migrated'),
(1084,133,'Main Partner Client Code'),
(1085,133,'Agent Main Code vw EID'),
(1086,133,'Registration Date'),
(1087,133,'Vertical'),
(1088,135,'Bank Name'),
(1089,135,'Factoring House'),
(1090,135,'Bank AccountNo'),
(1091,135,'IFSC Code'),
(1092,135,'Bank Account Type'),
(1093,135,'Payment Mode'),
(1094,138,'Branch Code'),
(1095,138,'Branch Name'),
(1096,139,'Confirmation Date'),
(1097,139,'Increment Date'),
(1098,139,'HR DOJ'),
(1099,139,'Last Working Date'),
(1100,139,'Last Promotion Date');

