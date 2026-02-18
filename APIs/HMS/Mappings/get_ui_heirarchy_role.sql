WITH flat_data AS (
    /*Combine components and fields into a single list*/
    SELECT 
        c.component_id, 
        c.path, 
        c.label, 
        c.elementType,
        nlevel(c.path) as lvl,
        COALESCE(
            jsonb_agg(
                jsonb_build_object(
                    'cntrlid', f.cntrl_id,
                    'cntrlName', f.cntrl_name,
                    'render', COALESCE(s.render, false),
                    'allowedit', COALESCE(s.allow_edit, false)
                ) ORDER BY s.sort_order, f.cntrl_id
            ) FILTER (WHERE f.cntrl_id IS NOT null), 
            '[]'::jsonb
        ) as field_list
    FROM hmsmaster.ui_components c
    LEFT JOIN hmsmaster.ui_fields f ON c.component_id = f.component_id
    LEFT JOIN hmsmaster.ui_fields_setting s ON f.cntrl_id = s.cntrl_id AND s.orgid = :p_orgId and s.role_id = :p_RoleId 
    GROUP BY c.component_id, c.path, c.label, c.elementType
),
level_3 AS (
    /*Step 2: Aggregate the deepest nodes (Sections)*/
    SELECT 
        subpath(path, 0, lvl - 1) as parent_path,
        jsonb_agg(
            jsonb_strip_nulls(jsonb_build_object(
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
    /*Step 3: Join Sections into Tabs*/
    SELECT 
        subpath(f.path, 0, f.lvl - 1) as parent_path,
        jsonb_agg(
            jsonb_strip_nulls(jsonb_build_object(
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
/*Step 4: Final output (Join Tabs into Screen)*/
SELECT jsonb_build_object(
    'UIMenu', jsonb_agg(
        jsonb_strip_nulls(jsonb_build_object(
            'Section', f.label,
            'Type', f.elementType,
            'SubSection', l2.children
        ))
    )
) AS result
FROM flat_data f
LEFT JOIN level_2 l2 ON f.path = l2.parent_path
WHERE f.lvl = 1;