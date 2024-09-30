using AutoMapper;
using Unnati.Helper;
using Unnati.Models;
using Unnati.Repos;
using Unnati.Repos.Models;
using Unnati.Service;
using Microsoft.EntityFrameworkCore;

namespace Unnati.Container
{
    public class UserService : IUserService
    {
        private readonly UnnatiContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;
        public UserService(UnnatiContext learndata, IMapper mapper, IEmailService emailService ,ILogger<UserService> logger)
        {
            this._context = learndata;
            this._mapper = mapper;
            this._emailService = emailService;
            this._logger = logger;
        }
        public async Task<APIResponse> ConfirmRegister(int userid, string username, string otptext)
        {
            APIResponse response = new APIResponse();
            bool otpresponse = await ValidateOTP(username, otptext);
            if (!otpresponse)
            {
                response.Result = "OTP Validation failed";
                response.Message = "Invalid OTP or Expired";
                _logger.LogInformation("Invalid OTP or Expired");
            }
            else
            {
                var _tempdata = await this._context.TblTempusers.FirstOrDefaultAsync(item => item.Id == userid);
                var _user = new TblUser()
                {
                    Username = username,
                    Name = _tempdata.Name,
                    Password = _tempdata.Password,
                    Email = _tempdata.Email,
                    Phone = _tempdata.Phone,
                    Failattempt = 0,
                    Isactive = true,
                    Islocked = false,
                    Role = "user"
                };
                await this._context.TblUsers.AddAsync(_user);
                await this._context.SaveChangesAsync();
                await UpdatePWDManager(username, _tempdata.Password);

                response.Result = "OTP Validated";
                response.Message = "Registered successfully.";
            }

            return response;
        }

        public async Task<APIResponse> UserRegisteration(UserRegister userRegister)
        {
            APIResponse response = new APIResponse();
            int userid = 0;
            bool isvalid = true;

            try
            {
                // duplicate user
                var _user = await this._context.TblUsers.Where(item => item.Username == userRegister.UserName).ToListAsync();
                if (_user.Count > 0)
                {
                    isvalid = false;
                    response.Result = "fail";
                    response.Message = "Duplicate username";
                    _logger.LogInformation("Duplicate username");
                }

                // duplicate Email
                var _useremail = await this._context.TblUsers.Where(item => item.Email == userRegister.Email).ToListAsync();
                if (_useremail.Count > 0)
                {
                    isvalid = false;
                    response.Result = "fail";
                    response.Message = "Duplicate Email";
                    _logger.LogInformation("Duplicate Email");

                }

                if (userRegister != null && isvalid)
                {
                    var _tempuser = new TblTempuser()
                    {
                        Code = userRegister.UserName,
                        Name = userRegister.Name,
                        Email = userRegister.Email,
                        Password = userRegister.Password,
                        Phone = userRegister.Phone,
                    };

                    await this._context.TblTempusers.AddAsync(_tempuser);
                    await this._context.SaveChangesAsync();
                    userid = _tempuser.Id;
                    string OTPText = Generaterandomnumber();
                    await UpdateOtp(userRegister.UserName, OTPText, "register");
                    await SendOtpMail(userRegister.Email, OTPText, userRegister.Name);


                    response.Result = "User successfully registered for Id + "+ userid;
                    response.Message = "OTP has been sent to "+ userRegister.Email;
                }
            }
            catch (Exception ex)
            {
                response.Result = "fail";
                response.Message = ex.Message;

                _logger.LogError(ex.Message,ex);

            }
            return response;
        }

        public async Task<APIResponse> ResetPassword(string username, string oldpassword, string newpassword)
        {
            APIResponse response = new APIResponse();
            var _user = await this._context.TblUsers.FirstOrDefaultAsync(item => item.Username == username &&
            item.Password == oldpassword && item.Isactive == true);
            if (_user != null)
            {
                var _pwdhistory = await Validatepwdhistory(username, newpassword);
                if (_pwdhistory)
                {
                    response.Result = "fail";
                    response.Message = "Don't use the same password that used in last 3 transaction";
                    _logger.LogInformation("Don't use the same password that used in last 3 transaction");
                }
                else
                {
                    _user.Password = newpassword;
                    await this._context.SaveChangesAsync();
                    await UpdatePWDManager(username, newpassword);
                    response.Result = "pass";
                    response.Message = "Password changed.";
                    _logger.LogInformation("Password changed");
                }
            }
            else
            {
                response.Result = "fail";
                response.Message = "Failed to validate old password.";

                _logger.LogError("Failed to validate old password.");
            }
            return response;
        }

        public async Task<APIResponse> ForgetPassword(string username)
        {
            APIResponse response = new APIResponse();
            var _user = await this._context.TblUsers.FirstOrDefaultAsync(item => item.Username == username && item.Isactive == true);
            if (_user != null)
            {
                string otptext = Generaterandomnumber();
                await UpdateOtp(username, otptext, "forgetpassword");
                await SendOtpMail(_user.Email, otptext, _user.Name);
                response.Result = "pass";
                response.Message = "OTP sent";
                _logger.LogInformation("OTP snet cueesflly");

            }
            else
            {
                response.Result = "fail";
                response.Message = "Invalid User";

                _logger.LogError("Invalid User");
            }
            return response;
        }

        public async Task<APIResponse> UpdatePassword(string username, string Password, string Otptext)
        {
            APIResponse response = new APIResponse();

            bool otpvalidation = await ValidateOTP(username, Otptext);
            if (otpvalidation)
            {
                bool pwdhistory = await Validatepwdhistory(username, Password);
                if (pwdhistory)
                {
                    response.Result = "fail";
                    response.Message = "Don't use the same password that used in last 3 transaction";
                }
                else
                {
                    var _user = await this._context.TblUsers.FirstOrDefaultAsync(item => item.Username == username && item.Isactive == true);
                    if (_user != null)
                    {
                        _user.Password = Password;
                        await this._context.SaveChangesAsync();
                        await UpdatePWDManager(username, Password);
                        response.Result = "pass";
                        response.Message = "Password changed";
                    }
                }
            }
            else
            {
                response.Result = "fail";
                response.Message = "Invalid OTP";
                _logger.LogError("Invalid OTP");
            }
            return response;
        }
        
        
        //Helpers
        private async Task UpdateOtp(string username, string otptext, string otptype)
        {
            var _opt = new TblOtpManager()
            {
                Username = username,
                Otptext = otptext,
                Expiration = DateTime.Now.AddMinutes(30),
                Createddate = DateTime.Now,
                Otptype = otptype
            };
            await this._context.TblOtpManagers.AddAsync(_opt);
            await this._context.SaveChangesAsync();
        }

        private async Task<bool> ValidateOTP(string username, string OTPText)
        {
            bool response = false;
            var _data = await this._context.TblOtpManagers.FirstOrDefaultAsync(item => item.Username == username
            && item.Otptext == OTPText && item.Expiration > DateTime.Now);
            if (_data != null)
            {
                response = true;
            }
            return response;
        }

        private async Task UpdatePWDManager(string username, string password)
        {
            var _opt = new TblPwdManger()
            {
                Username = username,
                Password = password,
                ModifyDate = DateTime.Now
            };
            await this._context.TblPwdMangers.AddAsync(_opt);
            await this._context.SaveChangesAsync();
        }

        private string Generaterandomnumber()
        {
            Random random = new Random();
            string randomno = random.Next(0, 1000000).ToString("D6");
            return randomno;
        }

        private async Task SendOtpMail(string useremail, string OtpText, string Name)
        {
            var mailrequest = new Mailrequest();
            mailrequest.Email = useremail;
            mailrequest.Subject = "Thanks for registering : OTP";
            mailrequest.Emailbody = GenerateEmailBody(Name, OtpText);
            await this._emailService.SendEmail(mailrequest);

        }

        private string GenerateEmailBody(string name, string otptext)
        {
            string emailbody = "<div style='width:100%;background-color:grey'>";
            emailbody += "<h1>Hi " + name + ", Thanks for registering</h1>";
            emailbody += "<h2>Please enter OTP text and complete the registeration</h2>";
            emailbody += "<h2>OTP Text is :" + otptext + "</h2>";
            emailbody += "</div>";

            return emailbody;
        }

        private async Task<bool> Validatepwdhistory(string Username, string password)
        {
            bool response = false;
            var _pwd = await this._context.TblPwdMangers.Where(item => item.Username == Username).
                OrderByDescending(p => p.ModifyDate).Take(3).ToListAsync();
            if (_pwd.Count > 0)
            {
                var validate = _pwd.Where(o => o.Password == password);
                if (validate.Any())
                {
                    response = true;
                }
            }

            return response;

        }

        public async Task<APIResponse> UpdateStatus(string username, bool userstatus)
        {
            APIResponse response = new APIResponse();
            var _user = await this._context.TblUsers.FirstOrDefaultAsync(item => item.Username == username);
            if (_user != null)
            {
                _user.Isactive = userstatus;
                await this._context.SaveChangesAsync();
                response.Result = "pass";
                response.Message = "User Status changed";
            }
            else
            {
                response.Result = "fail";
                response.Message = "Invalid User";
            }
            return response;
        }

        public async Task<APIResponse> UpdateRole(string username, string userrole)
        {
            APIResponse response = new APIResponse();
            var _user = await this._context.TblUsers.FirstOrDefaultAsync(item => item.Username == username && item.Isactive == true);
            if (_user != null)
            {
                _user.Role = userrole;
                await this._context.SaveChangesAsync();
                response.Result = "pass";
                response.Message = "User Role changed";
            }
            else
            {
                response.Result = "fail";
                response.Message = "Invalid User";
            }
            return response;
        }

        public async Task<List<UserModel>> Getall()
        {
            List<UserModel> _response = new List<UserModel>();
            var _data = await this._context.TblUsers.ToListAsync();
            if (_data != null)
            {
                _response = this._mapper.Map<List<TblUser>, List<UserModel>>(_data);
            }
            return _response;
        }
        public async Task<UserModel> Getbycode(string code)
        {
            UserModel _response = new UserModel();
            var _data = await this._context.TblUsers.FindAsync(code);
            if (_data != null)
            {
                _response = this._mapper.Map<TblUser, UserModel>(_data);
            }
            return _response;
        }
    }
}
