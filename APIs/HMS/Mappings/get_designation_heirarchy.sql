WITH RECURSIVE designation_tree AS (
    -- 1. Root Nodes: Find the top-level designations (where path length is 1)
    -- Also apply your filters here (OrgId, Channel, SubChannel)
    SELECT 
        designation_id,
        designation_code,
        designation_name,
        hierarchy_path,
        1 AS level,
        designation_name::text AS display_path
    FROM hmsmaster.designation_master
    WHERE orgid = @orgid 
      AND channel_id = @channel_id 
      AND sub_channel_id = @sub_channel_id
      AND nlevel(hierarchy_path) = 1

    UNION ALL

    -- 2. Recursive Step: Join children to their parents
    SELECT 
        child.designation_id,
        child.designation_code,
        child.designation_name,
        child.hierarchy_path,
        parent.level + 1,
        parent.display_path || ' -> ' || child.designation_name
    FROM hmsmaster.designation_master child
    INNER JOIN designation_tree parent ON subpath(child.hierarchy_path, 0, -1) = parent.hierarchy_path
    WHERE child.orgid = @orgid 
      AND child.channel_id = @channel_id 
      AND child.sub_channel_id = @sub_channel_id
)
SELECT * FROM designation_tree 
ORDER BY hierarchy_path;