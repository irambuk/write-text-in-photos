using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToExcel;

namespace QMora.FamilyPhotos.Data
{
    public class ParticipantDetailProvider
    {
        public IList<ParticipantRecord> GetAll(ParticipantRecord serachCriteria)
        {
            var excel = new ExcelQueryFactory(Properties.Settings.Default.File_ExcelData);
            //excel.AddTransformation<ParticipantRecord>(x => x.IsBankrupt, cellValue => cellValue == "Y");

            IQueryable<ParticipantRecord> query = excel.Worksheet<ParticipantRecord>();

            if (!string.IsNullOrEmpty(serachCriteria.No))
            {
                query = query.Where(p => p.No.Contains(serachCriteria.No));
            }
            if (!string.IsNullOrEmpty(serachCriteria.Name))
            {
                query = query.Where(p => p.Name.Contains(serachCriteria.Name));
            }
            if (!string.IsNullOrEmpty(serachCriteria.GraduatedYear))
            {
                query = query.Where(p => p.GraduatedYear.Contains(serachCriteria.GraduatedYear));
            }
            if (!string.IsNullOrEmpty(serachCriteria.Faculty))
            {
                query = query.Where(p => p.Faculty.Contains(serachCriteria.Faculty));
            }

            return query.ToList();
        }
    }
}
