using Data.DataAccess;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core
{
    public interface IReportService
    {
        public Task<ResultModel> Get(int pageSize = 10, int pageIndex = 0, DateTime? from = null, DateTime? to = null, string province = null);
        public Task<ResultModel> Calculate(int year, int month);
        public Task<ResultModel> SentToPQM(int year, int month, string province);
    };

    public class ReportService : IReportService
    {
        private ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<IndicatorData> GetReport(string ageGroup, string gender, string district, string site, List<SQi> forms)
        {
            var result = new List<IndicatorData>();
            //Clients_ExpStigma
            result.Add(new IndicatorData
            {
                district_code = district,
                indicator_code = "Clients_ExpStigma",
                site_code = site,
                data = new Detail
                {
                    age_group = ageGroup,
                    sex = gender,
                    key_population = "N/A",
                    type = "month",
                    value = forms.Count(s => s.Question1 > 2 || s.Question2 == 1).ToString(),
                    _value = forms.Count(s => s.Question1 > 2 || s.Question2 == 1),
                    denominatorValue = forms.Count(s => s.Question1 > 0 && s.Question2 > 0)
                },
                optional_data = new OptionalData
                {
                    value = forms.Count(s => s.Question1 > 0 && s.Question2 > 0).ToString(),
                    _value = forms.Count(s => s.Question1 > 0 && s.Question2 > 0)
                }
            });
            //Clients_NoExpStigma
            result.Add(new IndicatorData
            {
                district_code = district,
                indicator_code = "Clients_NoExpStigma",
                site_code = site,
                data = new Detail
                {
                    age_group = ageGroup,
                    sex = gender,
                    key_population = "N/A",
                    type = "month",
                    value = (forms.Count(s => s.Question1 > 0 && s.Question2 > 0) - forms.Count(s => s.Question1 > 2 || s.Question2 == 1)).ToString(),
                    _value = forms.Count(s => s.Question1 > 0 && s.Question2 > 0) - forms.Count(s => s.Question1 > 2 || s.Question2 == 1),
                    denominatorValue = forms.Count(s => s.Question1 > 0 && s.Question2 > 0)
                },
                optional_data = new OptionalData
                {
                    value = forms.Count(s => s.Question1 > 0 && s.Question2 > 0).ToString(),
                    _value = forms.Count(s => s.Question1 > 0 && s.Question2 > 0)
                }
            });
            //Clients_knowVL
            result.Add(new IndicatorData
            {
                district_code = district,
                indicator_code = "Clients_knowVL",
                site_code = site,
                data = new Detail
                {
                    age_group = ageGroup,
                    sex = gender,
                    key_population = "N/A",
                    type = "month",
                    value = forms.Count(s => s.Question5 == 1).ToString(),
                    _value = forms.Count(s => s.Question5 == 1),
                    denominatorValue = forms.Count(s => s.Question5 == 2 || s.Question5 == 1)
                },
                optional_data = new OptionalData
                {
                    value = forms.Count(s => s.Question5 == 2 || s.Question5 == 1).ToString(),
                    _value = forms.Count(s => s.Question5 == 2 || s.Question5 == 1)
                }
            });
            //Clients_ExpIPV
            result.Add(new IndicatorData
            {
                district_code = district,
                indicator_code = "Clients_ExpIPV",
                site_code = site,
                data = new Detail
                {
                    age_group = ageGroup,
                    sex = gender,
                    key_population = "N/A",
                    type = "month",
                    value = forms.Count(s => s.Question6 == 1 && s.Question7.Any(a => a == 1 || a == 2 || a == 3)).ToString(),
                    _value = forms.Count(s => s.Question6 == 1 && s.Question7.Any(a => a == 1 || a == 2 || a == 3)),
                    denominatorValue = forms.Count(s => s.Question6 == 1)
                },
                optional_data = new OptionalData
                {
                    value = forms.Count(s => s.Question6 == 1).ToString(),
                    _value = forms.Count(s => s.Question6 == 1)
                }
            });
            //Clients_NoExpIPV
            result.Add(new IndicatorData
            {
                district_code = district,
                indicator_code = "Clients_NoExpIPV",
                site_code = site,
                data = new Detail
                {
                    age_group = ageGroup,
                    sex = gender,
                    key_population = "N/A",
                    type = "month",
                    value = (forms.Count(s => s.Question6 == 1) - forms.Count(s => s.Question6 == 1 && s.Question7.Any(a => a == 1 || a == 2 || a == 3))).ToString(),
                    _value = forms.Count(s => s.Question6 == 1) - forms.Count(s => s.Question6 == 1 && s.Question7.Any(a => a == 1 || a == 2 || a == 3)),
                    denominatorValue = forms.Count(s => s.Question6 == 1)
                },
                optional_data = new OptionalData
                {
                    value = forms.Count(s => s.Question6 == 1).ToString(),
                    _value = forms.Count(s => s.Question6 == 1)
                }
            });
            return result;
        }

        public async Task<ResultModel> Calculate(int year, int month)
        {
            var rs = new ResultModel();
            try
            {
                var from = new DateTime(year, month, 1).AddHours(-7);
                var to = new DateTime(year, month, DateTime.DaysInMonth(year, month)).AddHours(-7);
                var basefilter = Builders<SQi>.Filter.Gte(mt => mt.SurveyDate, from) & Builders<SQi>.Filter.Lt(mt => mt.SurveyDate, to);
                var forms = await (await _context.SQiCollection.FindAsync(basefilter)).ToListAsync();
                if (forms.Count == 0)
                {
                    return rs;
                }
                var reports = new List<AggregateData>();
                forms.GroupBy(p => p.Province).ToList().ForEach(p =>
                {
                    var province = p.Key;
                    var report = new AggregateData
                    {
                        year = year.ToString(),
                        month = month.ToString(),
                        province_code = province,
                        date = new DateTime(year, month, 1),
                        datas = new List<IndicatorData>(),
                        IsSync = false,
                    };
                    p.GroupBy(s => s.Facility).ToList().ForEach(s =>
                    {
                        var site = s.Key;
                        var district = s.First().District;
                        s.GroupBy(s => s.AgeGroup).ToList().ForEach(a =>
                        {
                            var ageGroup = a.Key;
                            a.GroupBy(g => g.Gender).ToList().ForEach(g =>
                            {
                                var gender = g.Key == 1 ? "Male" : g.Key == 2 ? "Female" : "N/A";
                                report.datas.AddRange(GetReport(ageGroup, gender, district, site, g.ToList()));
                            });
                        });
                    });
                    reports.Add(report);
                });
                var session = _context.StartSession();
                session.StartTransaction();
                var _month = month.ToString();
                var _year = year.ToString();
                _context.ReportCollection.DeleteMany(s => s.month == _month && s.year == _year);
                _context.ReportCollection.InsertMany(reports);
                session.CommitTransaction();
                rs.Succeed = true;
            }
            catch (Exception e)
            {
                rs.Succeed = false;
                rs.ErrorMessage = e.Message;
            }
            return rs;
        }

        public async Task<ResultModel> Get(int pageSize = 10, int pageIndex = 0, DateTime? from = null, DateTime? to = null, string province = null)
        {
            var result = new PaginationResultModel();
            try
            {
                var basefilter = Builders<AggregateData>.Filter.Empty;
                if (from.HasValue)
                {
                    var _from = from.Value.Date;
                    var fromFilter = Builders<AggregateData>.Filter.Gte(mt => mt.date, _from);
                    basefilter = basefilter & fromFilter;
                }
                if (to.HasValue)
                {
                    var _to = to.Value.Date.AddDays(1);
                    var toFilter = Builders<AggregateData>.Filter.Lt(mt => mt.date, _to);
                    basefilter = basefilter & toFilter;
                }
                if (!string.IsNullOrEmpty(province))
                {
                    var provinceFilter = Builders<AggregateData>.Filter.Eq(mt => mt.province_code, province);
                    basefilter = basefilter & provinceFilter;
                }
                var query = _context.ReportCollection.Find(basefilter).SortByDescending(mt => mt.date);
                var total = (int)await query.CountDocumentsAsync();
                result.Succeed = true;
                result.Data = await query.Skip(pageSize * pageIndex).Limit(pageSize).ToListAsync();
                result.Pagination = new Pagination
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = total,
                };
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ResultModel> SentToPQM(int year, int month, string province)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<AggregateData>.Filter.Eq(mt => mt.year, year.ToString())
                & Builders<AggregateData>.Filter.Eq(mt => mt.month, month.ToString())
                & Builders<AggregateData>.Filter.Eq(mt => mt.province_code, province);
                var report = _context.ReportCollection.Find(basefilter).FirstOrDefault();
                using (var _httpClient = new HttpClient())
                {
                    var url = province == "75" ? "https://pcdn.bakco.vn/api/AggregatedValues/ImportV2" :
                              province == "82" ? "https://pctg.bakco.vn/api/AggregatedValues/ImportV2" :
                              province == "72" ? "https://pctn.bakco.vn/api/AggregatedValues/ImportV2" :
                              province == "79" ? "https://pqm-core.hcdc.vn/api/AggregatedValues/ImportV2" : "";
                    if (!string.IsNullOrEmpty(url))
                    {
                        var response = await _httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(report), System.Text.Encoding.UTF8, "application/json"));
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var rs = JsonConvert.DeserializeObject<ResultModel>(content);
                            result.Succeed = rs.Succeed;
                            if (rs.Succeed)
                            {
                                report.IsSync = true;
                                _context.ReportCollection.ReplaceOne(s => s.Id == report.Id, report);
                            }
                            else
                            {
                                result.ErrorMessage = content;
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
