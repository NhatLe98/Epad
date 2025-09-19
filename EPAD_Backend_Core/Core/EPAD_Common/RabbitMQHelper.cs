using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Common
{
    public class RabbitMQHelper
    {
        private ILogger mLogger = null;
        private ConnectionFactory mFactory;
        private IConnection mConnection;
        private bool mUseMessageQueue = false;
        private string mHost = "";
        private string mPort = "";
        private string mUser = "";
        private string mPass = "";
        public RabbitMQHelper(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            mLogger = loggerFactory.CreateLogger<RabbitMQHelper>();
            mHost = configuration.GetValue<string>("RabbitMQ_Host");
            mPort = configuration.GetValue<string>("RabbitMQ_Port");
            mUser = configuration.GetValue<string>("RabbitMQ_User");
            mPass = configuration.GetValue<string>("RabbitMQ_Pass");
            if (mHost != null && mHost != "")
            {
                Init();
            }
        }
        private void Init()
        {
            try
            {
                mFactory = new ConnectionFactory() { HostName = mHost, Port = int.Parse(mPort), UserName = mUser, Password = mPass };
                mConnection = mFactory.CreateConnection();
                mUseMessageQueue = true;
            }
            catch(Exception ex)
            {
                mLogger.LogError(ex.ToString());
            }
        }
        public bool CheckUseMessageQueue()
        {
            return mUseMessageQueue;
        }
        public bool CreateMessageCommand<T>(int pCompanyIndex,List<T> pListData,string pMessageType)
        {
            if (pListData.Count == 0)
            {
                mLogger.LogError("NoData");
                return false;
            }
            if (CheckUseMessageQueue() == false)
            {
                mLogger.LogError("NotUseMQ");
                return false;
            }
            if (mConnection.IsOpen == false)
            {
                Init();
            }
            bool success = true;
            try
            {
                using (var channel = mConnection.CreateModel())
                {
                    channel.QueueDeclare(queue: "CommandMQ_" + pCompanyIndex,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);
                    
                    
                    for (int i = 0; i < pListData.Count; i++)
                    {

                        PropertyInfo[] arrProperty = pListData[i].GetType().GetProperties();
                        string id = "";
                        foreach (PropertyInfo item in arrProperty)
                        {
                            if (item.Name == "ID")
                            {
                                id = item.GetValue(pListData[i],null).ToString();
                                break;
                            }
                        }

                        if (id == "")
                        {
                            id = "NoID_" + DateTime.Now.ToString("yyyyMMddHH:mm:ss");
                        }
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;
                        properties.MessageId = id;

                        Dictionary<string, object> header = new Dictionary<string, object>();
                        header.Add("MessageType", pMessageType.ToString());
                        properties.Headers = header;

                        string jsonData = JsonConvert.SerializeObject(pListData[i]);
                        Byte[] body = Encoding.UTF8.GetBytes(jsonData);

                        channel.BasicPublish(exchange: "",
                                           routingKey: "CommandMQ_" + pCompanyIndex,
                                           basicProperties: properties,
                                           body: body);
                    }

                }
            }
            catch(Exception ex)
            {
                success = false;
                mLogger.LogError(ex.ToString());
            }

            return success;
        }
        public bool CreateMessageInfo<T>(int pServiceIndex, T pData, string pMessageType)
        {
            if (CheckUseMessageQueue() == false)
            {
                mLogger.LogError("NotUseMQ");
                return false;
            }
            if (mConnection.IsOpen == false)
            {
                Init();
            }
            bool success = true;
            try
            {
                using (var channel = mConnection.CreateModel())
                {
                    channel.QueueDeclare(queue: "Service_" + pServiceIndex,
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);
                    // check the consumers connected. if not consumer connected when not send the message
                    var queuePassive = channel.QueueDeclarePassive("Service_" + pServiceIndex);
                    if (queuePassive.ConsumerCount == 0)
                    {
                        //return false;
                    }

                    PropertyInfo[] arrProperty = pData.GetType().GetProperties();
                    string id = "";
                    foreach (PropertyInfo item in arrProperty)
                    {
                        if (item.Name == "ID")
                        {
                            id = item.GetValue(pData, null).ToString();
                            break;
                        }
                    }

                    if (id == "")
                    {
                        id = "NoID_" + DateTime.Now.ToString("yyyyMMddHH:mm:ss");
                    }
                    var properties = channel.CreateBasicProperties();
                    properties.MessageId = id;

                    Dictionary<string, object> header = new Dictionary<string, object>();
                    header.Add("MessageType", pMessageType.ToString());
                    properties.Headers = header;

                    string jsonData = JsonConvert.SerializeObject(pData);
                    Byte[] body = Encoding.UTF8.GetBytes(jsonData);

                    channel.BasicPublish(exchange: "",
                                       routingKey: "Service_" + pServiceIndex,
                                       basicProperties: properties,
                                       body: body);
                }
            }
            catch(Exception ex)
            {
                success = false;
                mLogger.LogError(ex.ToString());
            }
            return success;
        }
    }
}
