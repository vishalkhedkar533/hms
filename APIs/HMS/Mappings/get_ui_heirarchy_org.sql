WITH filtered_components AS (
    /* Step 1: Identify components that ARE or HAVE descendants with approval settings */
    SELECT DISTINCT c.*
    FROM hmsmaster.ui_components c
    JOIN hmsmaster.ui_components leaf ON leaf.path <@ c.path
    LEFT JOIN hmsmaster.approval_setting f ON leaf.component_id = f.component_id and f.orgid = :p_orgId 
    WHERE c.elementtype IN ('Screen','Tab','Section')
),
flat_data AS (
    /* Step 2: Aggregate the specific field data */
    SELECT 
        c.component_id, 
        c.path, 
        c.label, 
        c.elementType,
        nlevel(c.path) as lvl,
        COALESCE(
            jsonb_agg(
                jsonb_build_object(
                    'componentId', f.component_id,
                    'componentName', c."label",
                    'approverOneRoleId', f.approveroneid,
                    'approverTwoRoleId', f.approvertwoid,
                    'approverThreeRoleId', f.approverthreeid,
                    'useDefaultApprover', f.usedefaultapprover,
                    'isLog', f.is_log
                ) ORDER BY f.component_id
            ) FILTER (WHERE f.component_id IS NOT NULL), 
            '[]'::jsonb
        ) as field_list
    FROM filtered_components c
    LEFT JOIN hmsmaster.approval_setting f ON c.component_id = f.component_id AND f.orgid = :p_orgId
    GROUP BY c.component_id, c.path, c.label, c.elementType
),
level_3 AS (
    /* Step 3: Aggregate deepest nodes */
    SELECT 
        subpath(path, 0, nlevel(path) - 1) as parent_path,
        jsonb_agg(
            jsonb_strip_nulls(jsonb_build_object(
                'componentId', component_id,
                'Section', label,
                'Type', elementType,
                'FieldList', CASE WHEN jsonb_array_length(field_list) > 0 THEN field_list ELSE NULL END
            ))
        ) as children
    FROM flat_data
    WHERE lvl = 3
    GROUP BY 1
),
level_2 AS (
    /* Step 4: Join Sections into Tabs */
    SELECT 
        subpath(f.path, 0, nlevel(f.path) - 1) as parent_path,
        jsonb_agg(
            jsonb_strip_nulls(jsonb_build_object(
                'componentId', component_id,
                'Section', f.label,
                'Type', f.elementType,
                'SubSection', l3.children
            ))
        ) as children
    FROM flat_data f
    LEFT JOIN level_3 l3 ON f.path = l3.parent_path
    WHERE f.lvl = 2
    GROUP BY 1
)
/* Step 5: Final output (Join Tabs into Screen) */
SELECT jsonb_build_object(
    'UIMenu', jsonb_agg(
        jsonb_strip_nulls(jsonb_build_object(
            'componentId', f.component_id,
            'Section', f.label,
            'Type', f.elementType,
            'SubSection', l2.children
        ))
    )
) AS result
FROM flat_data f
LEFT JOIN level_2 l2 ON f.path = l2.parent_path
WHERE f.lvl = 1;