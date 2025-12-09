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