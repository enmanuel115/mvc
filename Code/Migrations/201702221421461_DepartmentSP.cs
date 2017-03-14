namespace Code.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DepartmentSP : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.InstructorCourse", newName: "CourseInstructor");
            RenameColumn(table: "dbo.CourseInstructor", name: "Instructor_ID", newName: "InstructorID");
            RenameColumn(table: "dbo.CourseInstructor", name: "Course_CourseID", newName: "CurseID");
            RenameIndex(table: "dbo.CourseInstructor", name: "IX_Course_CourseID", newName: "IX_CurseID");
            RenameIndex(table: "dbo.CourseInstructor", name: "IX_Instructor_ID", newName: "IX_InstructorID");
            DropPrimaryKey("dbo.CourseInstructor");
            AddPrimaryKey("dbo.CourseInstructor", new[] { "CurseID", "InstructorID" });
            CreateStoredProcedure(
                "dbo.Department_Insert",    
                p => new
                    {
                        Name = p.String(maxLength: 50),
                        Budget = p.Decimal(precision: 19, scale: 4, storeType: "money"),
                        StartDate = p.DateTime(),
                        InstructorID = p.Int(),
                    },
                body:
                    @"INSERT [dbo].[Department]([Name], [Budget], [StartDate], [InstructorID])
                      VALUES (@Name, @Budget, @StartDate, @InstructorID)
                      
                      DECLARE @DepartmentID int
                      SELECT @DepartmentID = [DepartmentID]
                      FROM [dbo].[Department]
                      WHERE @@ROWCOUNT > 0 AND [DepartmentID] = scope_identity()
                      
                      SELECT t0.[DepartmentID]
                      FROM [dbo].[Department] AS t0
                      WHERE @@ROWCOUNT > 0 AND t0.[DepartmentID] = @DepartmentID"
            );
            
            CreateStoredProcedure(
                "dbo.Department_Update",
                p => new
                    {
                        DepartmentID = p.Int(),
                        Name = p.String(maxLength: 50),
                        Budget = p.Decimal(precision: 19, scale: 4, storeType: "money"),
                        StartDate = p.DateTime(),
                        InstructorID = p.Int(),
                    },
                body:
                    @"UPDATE [dbo].[Department]
                      SET [Name] = @Name, [Budget] = @Budget, [StartDate] = @StartDate, [InstructorID] = @InstructorID
                      WHERE ([DepartmentID] = @DepartmentID)"
            );
            
            CreateStoredProcedure(
                "dbo.Department_Delete",
                p => new
                    {
                        DepartmentID = p.Int(),
                    },
                body:
                    @"DELETE [dbo].[Department]
                      WHERE ([DepartmentID] = @DepartmentID)"
            );
            
        }
        
        public override void Down()
        {
            DropStoredProcedure("dbo.Department_Delete");
            DropStoredProcedure("dbo.Department_Update");
            DropStoredProcedure("dbo.Department_Insert");
            DropPrimaryKey("dbo.CourseInstructor");
            AddPrimaryKey("dbo.CourseInstructor", new[] { "Instructor_ID", "Course_CourseID" });
            RenameIndex(table: "dbo.CourseInstructor", name: "IX_InstructorID", newName: "IX_Instructor_ID");
            RenameIndex(table: "dbo.CourseInstructor", name: "IX_CurseID", newName: "IX_Course_CourseID");
            RenameColumn(table: "dbo.CourseInstructor", name: "CurseID", newName: "Course_CourseID");
            RenameColumn(table: "dbo.CourseInstructor", name: "InstructorID", newName: "Instructor_ID");
            RenameTable(name: "dbo.CourseInstructor", newName: "InstructorCourse");
        }
    }
}
