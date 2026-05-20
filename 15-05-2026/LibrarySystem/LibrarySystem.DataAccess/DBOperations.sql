CREATE OR REPLACE FUNCTION calculate_member_fine(p_member_id INT)
RETURNS DECIMAL AS $$
DECLARE
    v_total_fine DECIMAL := 0;
BEGIN
    SELECT COALESCE(SUM("FineAmount"), 0)
    INTO v_total_fine
    FROM "Fines"
    WHERE "MemberId" = p_member_id
    AND "PaymentStatus" = 'Pending'
    AND "IsActive" = true;

    RETURN v_total_fine;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION get_member_borrowing_summary(p_member_id INT)
RETURNS TABLE(
    active_borrowings INT,
    returned_borrowings INT,
    overdue_borrowings INT,
    total_fine DECIMAL
) AS $$
DECLARE
    v_active_count INT := 0;
    v_returned_count INT := 0;
    v_overdue_count INT := 0;
    v_total_fine DECIMAL := 0;
BEGIN

    -- Active borrowings
    SELECT COUNT(*)
    INTO v_active_count
    FROM "Borrowings"
    WHERE "MemberId" = p_member_id
    AND "Status" = 'Borrowed'
    AND "IsActive" = true;

    -- Returned borrowings
    SELECT COUNT(*)
    INTO v_returned_count
    FROM "Borrowings"
    WHERE "MemberId" = p_member_id
    AND "Status" = 'Returned'
    AND "IsActive" = true;

    -- Overdue borrowings
    SELECT COUNT(*)
    INTO v_overdue_count
    FROM "Borrowings"
    WHERE "MemberId" = p_member_id
    AND "Status" = 'Borrowed'
    AND "DueDate" < NOW()
    AND "IsActive" = true;

    -- Total unpaid fine
    SELECT COALESCE(SUM("FineAmount"), 0)
    INTO v_total_fine
    FROM "Fines"
    WHERE "MemberId" = p_member_id
    AND "PaymentStatus" = 'Pending'
    AND "IsActive" = true;

    RETURN QUERY
    SELECT
        v_active_count,
        v_returned_count,
        v_overdue_count,
        v_total_fine;

END;
$$ LANGUAGE plpgsql;


