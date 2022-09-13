using AutoMapper;
using Data.Constants;
using Data.DataAccess;
using Data.Enums;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Driver;
using Newtonsoft.Json;
using Services.RabbitMQ;
using Services.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Net.Http;

namespace Services.Core
{
    public interface ISQiService
    {
        Task<ResultModel> Add(SQiCreateModel model, string username);
        Task<PaginationResultModel> Get(int pageSize = 10, int pageIndex = 0, DateTime? from = null, DateTime? to = null, string user = null);
        Task<ResultModel> ExportExcel(DateTime? from = null, DateTime? to = null, string user = null);
        Task<ResultModel> GetStatistics(DateTime? from = null, DateTime? to = null, string user = null);
        //Task<ResultModel> Update(SQiUpdateModel model);
        ResultModel FixSiteCode(List<FixSiteCodeModel> fixSiteCodeModels);
        Task<ResultModel> Delete(Guid id);
    }

    public class SQiService : ISQiService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private List<AgeGroup> _ageGroups;

        public SQiService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _ageGroups = new List<AgeGroup>
            {
                new AgeGroup
                {
                    Name = "< 10",
                    From = 0,
                    To = 10
                },
                new AgeGroup
                {
                    Name = "10-14",
                    From = 10,
                    To = 14,
                },
                new AgeGroup
                {
                    Name = "15-19",
                    From = 15,
                    To = 19
                },
                new AgeGroup
                {
                    Name = "20-24",
                    From = 20,
                    To = 24
                },
                new AgeGroup
                {
                    Name = "25-29",
                    From = 25,
                    To = 29
                },
                new AgeGroup
                {
                    Name = "30-34",
                    From = 30,
                    To = 34
                },
                new AgeGroup
                {
                    Name = "35-39",
                    From = 35,
                    To = 39
                },
                new AgeGroup
                {
                    Name = "40-44",
                    From = 40,
                    To = 44
                },
                new AgeGroup
                {
                    Name = "45-49",
                    From = 45,
                    To = 49
                },
                new AgeGroup
                {
                    Name = "50+",
                    From = 50,
                    To = 1000,
                },
            };
        }

        private async Task<bool> SendToOtherApp(SQiCreateModel model)
        {
            var url = $"https://openhimcore-service.quanlyhiv.vn/api/sqi/sendform/{model.App}";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "c3FpOlphcUAxMjNBQkM=");
                    var rs = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(model), System.Text.Encoding.UTF8));
                    if (rs.IsSuccessStatusCode)
                    {
                        return true;

                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<ResultModel> Add(SQiCreateModel model, string username)
        {
            var result = new ResultModel();

            try
            {
                SQi newModel = _mapper.Map<SQiCreateModel, SQi>(model);
                newModel.User = username;
                foreach (var ageGroup in _ageGroups)
                {
                    if (ageGroup.From <= newModel.Age && newModel.Age <= ageGroup.To)
                    {
                        newModel.AgeGroup = ageGroup.Name;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(newModel.AgeGroup))
                {
                    newModel.AgeGroup = "N/A";
                }
                await _context.SQiCollection.InsertOneAsync(newModel);

                result.Data = _mapper.Map<SQi, SQiViewModel>(newModel);
                result.Succeed = true;
                await SendToOtherApp(model);
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        public async Task<ResultModel> Delete(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var e = await _context.SQiCollection.DeleteOneAsync(e => e.Id == id);
                if (e.DeletedCount > 0)
                {
                    result.Succeed = true;
                }
                else
                {
                    result.Succeed = false;
                }
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> ExportExcel(DateTime? from = null, DateTime? to = null, string user = null)
        {
            try
            {
                var basefilter = Builders<SQi>.Filter.Empty;
                if (from.HasValue)
                {
                    var _from = from.Value.Date;
                    var fromFilter = Builders<SQi>.Filter.Gte(mt => mt.SurveyDate, _from);
                    basefilter = basefilter & fromFilter;
                }
                if (to.HasValue)
                {
                    var _to = to.Value.Date.AddDays(1);
                    var toFilter = Builders<SQi>.Filter.Lt(mt => mt.SurveyDate, _to);
                    basefilter = basefilter & toFilter;
                }
                if (!(new List<string> { "longhdt", "sqi_admin", "admin" }).Any(s => s == user))
                {
                    var userFilter = Builders<SQi>.Filter.Eq(mt => mt.App, user);
                    basefilter = basefilter & userFilter;
                }
                var rs = await (await _context.SQiCollection.FindAsync(basefilter)).ToListAsync();


                using (var ms = new MemoryStream())
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet excelSheet = workbook.CreateSheet("Survey");

                    IRow row = null;
                    ICell cell = null;
                    int rowIndex = 0;
                    row = excelSheet.CreateRow(rowIndex++);
                    var cellIndex = 0;
                    (new List<string> {
                        "Survey Id",
                        "Province",
                        "District",
                        "App",
                        "Facility",
                        "FacilityName",
                        "Gender",
                        "Age",
                        "SurveyDate",
                        "Question1",
                        "Question2",
                        "Question3",
                        "Question4",
                        "Question5",
                        "Question6",
                        "Question7"
                    }).ForEach(t =>
                    {
                        cell = row.CreateCell(cellIndex++);
                        cell.SetCellValue(t);
                    });
                    rs.ForEach(r =>
                    {
                        row = excelSheet.CreateRow(rowIndex++);
                        var cellIndex = 0;
                        (new List<string>
                        {
                            r.Id.ToString(),
                            r.Province,
                            r.District,
                            r.App,
                            r.Facility,
                            r.FacilityName,
                            r.Gender.ToString(),
                            r.Age.ToString(),
                            r.SurveyDate.ToString(),
                            r.Question1.ToString(),
                            r.Question2.ToString(),
                            r.Question3.ToString(),
                            r.Question4.ToString(),
                            r.Question5.ToString(),
                            r.Question6.ToString(),
                            String.Join(", ", r.Question7.Select(t => t.ToString()))
                        }).ForEach(t =>
                        {
                            cell = row.CreateCell(cellIndex++);
                            cell.SetCellValue(t);
                        });
                    });

                    workbook.Write(ms);
                    return new ResultModel()
                    {
                        Succeed = true,
                        Data = ms.ToArray(),
                    };
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<PaginationResultModel> Get(int pageSize = 10, int pageIndex = 0, DateTime? from = null, DateTime? to = null, string user = null)
        {
            var result = new PaginationResultModel();
            try
            {
                var basefilter = Builders<SQi>.Filter.Empty;
                if (from.HasValue)
                {
                    var _from = from.Value.Date;
                    var fromFilter = Builders<SQi>.Filter.Gte(mt => mt.SurveyDate, _from);
                    basefilter = basefilter & fromFilter;
                }
                if (to.HasValue)
                {
                    var _to = to.Value.Date.AddDays(1);
                    var toFilter = Builders<SQi>.Filter.Lt(mt => mt.SurveyDate, _to);
                    basefilter = basefilter & toFilter;
                }
                if (!(new List<string> { "longhdt", "sqi_admin", "admin" }).Any(s => s == user))
                {
                    var userFilter = Builders<SQi>.Filter.Eq(mt => mt.App, user);
                    basefilter = basefilter & userFilter;
                }
                var query = _context.SQiCollection.Find(basefilter).SortByDescending(s => s.SurveyDate);
                var total = (int)await query.CountDocumentsAsync();
                var rs = await query.Skip(pageSize * pageIndex).Limit(pageSize).ToListAsync();
                result.Pagination = new Pagination
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = total,
                };
                result.Data = _mapper.Map<List<SQi>, List<SQiViewModel>>(rs);
                result.Succeed = true;
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ResultModel> GetStatistics(DateTime? from = null, DateTime? to = null, string user = null)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<SQi>.Filter.Empty;
                if (from.HasValue)
                {
                    var _from = from.Value.Date;
                    var fromFilter = Builders<SQi>.Filter.Gte(mt => mt.SurveyDate, _from);
                    basefilter = basefilter & fromFilter;
                }
                if (to.HasValue)
                {
                    var _to = to.Value.Date.AddDays(1);
                    var toFilter = Builders<SQi>.Filter.Lt(mt => mt.SurveyDate, _to);
                    basefilter = basefilter & toFilter;
                }
                if (!(new List<string> { "longhdt", "sqi_admin", "admin" }).Any(s => s == user))
                {
                    var userFilter = Builders<SQi>.Filter.Eq(mt => mt.App, user);
                    basefilter = basefilter & userFilter;
                }
                var rs = await (await _context.SQiCollection.FindAsync(basefilter)).ToListAsync();
                var clients_expStigma_n = rs.Count(s => s.Question1 > 2 || s.Question2 == 1);
                var clients_expStigma_d = rs.Count(s => s.Question1 > 0 && s.Question2 > 0);
                var clients_know_vl_n = rs.Count(s => s.Question5 == 1);
                var clients_know_vl_d = rs.Count(s => s.Question5 == 2 || s.Question5 == 1);
                var clients_exp_ipv_n = rs.Count(s => s.Question6 == 1 && s.Question7.Any(a => a == 1 || a == 2 || a == 3));
                var clients_exp_ipv_d = rs.Count(s => s.Question6 == 1);
                result.Data = new List<Object>
                {
                    new
                    {
                        Indicator = "Clients_ExpStigma",
                        Numerator = clients_expStigma_n,
                        Denominator = clients_expStigma_d
                    },
                    new
                    {
                        Indicator = "Clients_knowVL",
                        Numerator = clients_know_vl_n,
                        Denominator = clients_know_vl_d
                    },
                    new
                    {
                        Indicator = "Clients_ExpIPV",
                        Numerator = clients_exp_ipv_n,
                        Denominator = clients_exp_ipv_d
                    },
                };
                result.Succeed = true;
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ResultModel FixSiteCode(List<FixSiteCodeModel> fixSiteCodeModels)
        {
            var result = new ResultModel();
            try
            {
                fixSiteCodeModels.ForEach(item =>
                {
                    var e = _context.SQiCollection.Find(_ => _.Id == item.Id).FirstOrDefault();
                    e.Facility = item.Facility;
                    _context.SQiCollection.ReplaceOne(_ => _.Id == e.Id, e);
                   
                });
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }
    }
}
