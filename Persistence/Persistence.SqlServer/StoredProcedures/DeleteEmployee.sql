SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[DeleteEmployee]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[DeleteEmployee]
END
GO

-- =============================================
-- Detete employee including all his related objects
--	
CREATE PROCEDURE [dbo].[DeleteEmployee]
  -- the employee tenant
  @tenantId AS INT,
  -- the employee to delete
  @employeeId AS INT
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  -- transaction start
  BEGIN TRANSACTION;
  SAVE TRANSACTION DeleteEmployeeTransaction;

  BEGIN TRY
    -- employee payroll results
    DELETE [dbo].[PayrunResult]
    FROM [dbo].[PayrunResult]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[PayrunResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId AND
          [dbo].[PayrollResult].[EmployeeId] = @employeeId

    DELETE [dbo].[WageTypeCustomResult]
    FROM [dbo].[WageTypeCustomResult]
    INNER JOIN [dbo].[WageTypeResult]
      ON [dbo].[WageTypeCustomResult].[WageTypeResultId] = [dbo].[WageTypeResult].[Id]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId AND
          [dbo].[PayrollResult].[EmployeeId] = @employeeId

    DELETE [dbo].[WageTypeResult]
    FROM [dbo].[WageTypeResult]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId AND
          [dbo].[PayrollResult].[EmployeeId] = @employeeId

    DELETE [dbo].[CollectorCustomResult]
    FROM [dbo].[CollectorCustomResult]
    INNER JOIN [dbo].[CollectorResult]
      ON [dbo].[CollectorCustomResult].[CollectorResultId] = [dbo].[CollectorResult].[Id]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId AND
          [dbo].[PayrollResult].[EmployeeId] = @employeeId

    DELETE [dbo].[CollectorResult]
    FROM [dbo].[CollectorResult]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId AND
          [dbo].[PayrollResult].[EmployeeId] = @employeeId

    DELETE
    FROM [dbo].[PayrollResult]
    WHERE [TenantId] = @tenantId AND [EmployeeId] = @employeeId

    -- employee payrun jobs
    DELETE [dbo].[PayrunJobEmployee]
    FROM [dbo].[PayrunJobEmployee]
    INNER JOIN [dbo].[PayrunJob]
      ON [dbo].[PayrunJobEmployee].[PayrunJobId] = [dbo].[PayrunJob].[Id]
    WHERE [dbo].[PayrunJob].[TenantId] = @tenantId AND
          [dbo].[PayrunJobEmployee].[EmployeeId] = @employeeId

    -- employee
    DELETE [dbo].[EmployeeCaseValueChange]
    FROM [dbo].[EmployeeCaseValueChange]
    INNER JOIN [dbo].[EmployeeCaseChange]
      ON [dbo].[EmployeeCaseValueChange].[CaseChangeId] = [dbo].[EmployeeCaseChange].[Id]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeCaseChange].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId AND
          [dbo].[Employee].[Id] = @employeeId

    DELETE [dbo].[EmployeeCaseChange]
    FROM [dbo].[EmployeeCaseChange]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeCaseChange].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId AND
          [dbo].[Employee].[Id] = @employeeId

    DELETE [dbo].[EmployeeCaseDocument]
    FROM [dbo].[EmployeeCaseDocument]
    INNER JOIN [dbo].[EmployeeCaseValue]
      ON [dbo].[EmployeeCaseDocument].[CaseValueId] = [dbo].[EmployeeCaseValue].[Id]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeCaseValue].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId AND
          [dbo].[Employee].[Id] = @employeeId

    DELETE [dbo].[EmployeeCaseValue]
    FROM [dbo].[EmployeeCaseValue]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeCaseValue].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId AND
          [dbo].[Employee].[Id] = @employeeId

    DELETE [dbo].[EmployeeDivision]
    FROM [dbo].[EmployeeDivision]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeDivision].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId AND
          [dbo].[Employee].[Id] = @employeeId

    DELETE
    FROM [dbo].[Employee]
    WHERE [TenantId] = @tenantId AND [Id] = @employeeId

    -- transaction end
    COMMIT TRANSACTION;
    
    -- success
    RETURN 1
  END TRY

  BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
      ROLLBACK TRANSACTION DeleteEmployeeTransaction;
    END
    
    -- failure
    RETURN 0
  END CATCH
END
GO


