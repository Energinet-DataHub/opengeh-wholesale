ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT created_by_user_id_chk CHECK (LENGTH(created_by_user_id) = 36)
GO
