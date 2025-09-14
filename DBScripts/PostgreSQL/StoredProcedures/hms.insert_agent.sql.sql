CREATE OR REPLACE FUNCTION hms.insert_agent(
    p_agent_code VARCHAR,
    p_agent_type_code VARCHAR,
    p_agent_sub_type_code VARCHAR,
    p_agent_name VARCHAR,
    p_business_name VARCHAR,
    p_first_name VARCHAR,
    p_middle_name VARCHAR,
    p_last_name VARCHAR,
    p_prefix VARCHAR,
    p_suffix VARCHAR,
    p_gender VARCHAR,
    p_dob DATE,
    p_nationality VARCHAR,
    p_marital_status_code VARCHAR,
    p_preferred_language VARCHAR,
    p_channel_code VARCHAR,
    p_sub_channel_code VARCHAR,
    p_designation_code VARCHAR,
    p_agent_level VARCHAR,
    p_location_code VARCHAR,
    p_staff_code VARCHAR,
    p_supervisor_code VARCHAR,
    p_contracted_date DATE,
    p_agent_status_code VARCHAR,
    p_status_date DATE,
    p_is_licensed BOOLEAN,
    p_pan_number VARCHAR,
    p_aadhaar_number VARCHAR,
    p_irda_license_number VARCHAR,
    p_gst_number VARCHAR,
    p_created_by VARCHAR,
    p_email VARCHAR,
    p_mobile_no VARCHAR,
    OUT new_agent_id INT
)
RETURNS INT
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO hms.agent (
        agent_code, agent_type_code, agent_sub_type_code, agent_name,
        business_name, first_name, middle_name, last_name, prefix, suffix,
        gender, dob, nationality, marital_status_code, preferred_language,
        channel_code, sub_channel_code, designation_code, agent_level,
        location_code, staff_code, supervisor_code, contracted_date,
        agent_status_code, status_date, is_licensed, pan_number,
        aadhaar_number, irda_license_number, gst_number,
        created_by, created_date, email, mobileno
    )
    VALUES (
        p_agent_code, p_agent_type_code, p_agent_sub_type_code, p_agent_name,
        p_business_name, p_first_name, p_middle_name, p_last_name, p_prefix, p_suffix,
        p_gender, p_dob, p_nationality, p_marital_status_code, p_preferred_language,
        p_channel_code, p_sub_channel_code, p_designation_code, p_agent_level,
        p_location_code, p_staff_code, p_supervisor_code, p_contracted_date,
        p_agent_status_code, p_status_date, p_is_licensed, p_pan_number,
        p_aadhaar_number, p_irda_license_number, p_gst_number,
        p_created_by, NOW(), p_email, p_mobile_no
    )
    RETURNING agent_id INTO new_agent_id;

    RETURN new_agent_id;
END;
$$;
