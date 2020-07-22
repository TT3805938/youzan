# coding=utf-8
import  pandas  as pd
from openpyxl import Workbook
import time
import sys
import re,sys
class excel_data:
    def __init__(self,data,infoList):
        self.data=data
        self.infoList=infoList
def main(infoSrc='C:\\Users\\tiantao\\Desktop\\报表\\账单明细.xlsx',infoListSrc='C:\\Users\\tiantao\\Desktop\\报表\\Goods.csv',refSrc='C:\\Users\\tiantao\\Desktop\\报表\\Refund.csv',freightSrc=''):
    print(infoSrc,infoListSrc,refSrc)
    print("[开始]拿取账单明细")
    data =pd.read_excel(infoSrc)#('data/账单明细.xlsx')
    print("明细表共"+str(len(data))+"条数据")
    print("[结束]拿取账单明细")
    array=[]
    print("[开始]拿取明细表")
    array2=readCsv(infoListSrc)#明细表
    print("明细表共"+str(len(array2))+"条数据")
    print("[结束]拿取明细表")

    print("[开始]拿取退款明细表")
    ref_array=readrefCsv(refSrc)
    print("退款明细表共"+str(len(ref_array))+"条数据")
    print("[结束]拿取退款明细表")

    print("[开始]拿取运费明细表")
    freight_array=readFreightCsv(freightSrc)
    print("运费明细表共"+str(len(freight_array))+"条数据")
    print("[结束]拿取运费明细表")


    df = pd.DataFrame(data)
    print("[开始]清洗账单明细中数据")
    for index,item in df.iterrows():
        item["订单配送方式"]=""
        item["运费"]=""
        #print(index)
        #print(item['类型'])
        if item['类型']=="订单入账" or item['类型']=="退款":
            infolist=[]
            for info in array2:
                if item["关联单号"]==info["订单号"]:
                    #print(item["关联单号"])
                    info["退款金额"]=""
                    info["退款申请时间"]=""
                    for refInfo in ref_array:
                        if info["订单号"]==refInfo["订单编号"] and info["商品编码"]==refInfo["商品编码"]:
                            #print("退款金额:")
                            #print(refInfo["退款金额"])
                            info["退款金额"]=refInfo["退款金额"]
                            info["退款申请时间"]=refInfo["退款申请时间"]
                            break#此处怕有同一订单，同一编码退多次的情况出现
                    infolist.append(info)
            for freight in freight_array:
                if item["关联单号"]==freight["订单号"]:
                    item["订单配送方式"]=freight["订单配送方式"]
                    item["运费"]=freight["运费"]
            obj=excel_data(item,infolist)#[{"名称":"蛇草水","价格":"10.05","数量":"1"},{"名称":"矿泉水","价格":"8.05","数量":"1"}]
            array.append(obj)
    print("明细表共"+str(len(array))+"条数据")
    print("[结束]清洗账单明细中数据")
    time_stamp = time.strftime('%Y-%m-%d-%H-%M-%S',time.localtime(time.time()))
    addr =str(time_stamp)+".xlsx"#"openpyxl.xlsx"
    wb = Workbook()
    print("创建新excel成功")
    # 激活 worksheet
    ws1 = wb.create_sheet("Sheet1",0)
    print("创建新Sheet成功")
    ws1.append(["类型","名称","业务单号","支付流水号","关联单号","交易来源地","账务主体","账户","收入(元)","支出(元)","余额(元)","支付方式","交易对手","渠道","下单时间","入账时间","操作人","附加信息","备注","订单配送方式","运费","明细编码","明细名","明细数量","明细金额","退款金额","退款申请时间"])
    print("添加表头成功")
    print("[开始]组装订单明细详情并填充每行数据")
    for item in array:
        #print(item.infoList)
        if item.infoList==[]:
            ws1.append([item.data["类型"],item.data["名称"],item.data["业务单号"],item.data["支付流水号"],item.data["关联单号"],item.data["交易来源地"],item.data["账务主体"],item.data["账户"],item.data["收入(元)"],item.data["支出(元)"],item.data["余额(元)"],item.data["支付方式"],item.data["交易对手"],item.data["渠道"],item.data["下单时间"],item.data["入账时间"],item.data["操作人"],item.data["附加信息"],item.data["备注"],"订单配送方式","运费","数据丢失","数据丢失","数据丢失","数据丢失","数据丢失","数据丢失"])
        else:
            for i in range(0,len(item.infoList)):
                if i==0:
                    ws1.append([item.data["类型"],item.data["名称"],item.data["业务单号"],item.data["支付流水号"],item.data["关联单号"],item.data["交易来源地"],item.data["账务主体"],item.data["账户"],item.data["收入(元)"],item.data["支出(元)"],item.data["余额(元)"],item.data["支付方式"],item.data["交易对手"],item.data["渠道"],item.data["下单时间"],item.data["入账时间"],item.data["操作人"],item.data["附加信息"],item.data["备注"],item.data["订单配送方式"],item.data["运费"],item.infoList[i]["商品编码"],item.infoList[i]["名称"],item.infoList[i]["数量"],item.infoList[i]["价格"],item.infoList[i]["退款金额"],item.infoList[i]["退款申请时间"]])
                else:
                    ws1.append(["","","","","","","","","","","","","","","","","","","","","",item.infoList[i]["商品编码"],item.infoList[i]["名称"],item.infoList[i]["数量"],item.infoList[i]["价格"],item.infoList[i]["退款金额"],item.infoList[i]["退款申请时间"]])
    print("[结束]组装订单明细详情并填充每行数据")
    wb.save(addr)
    print("excel保存成功,程序结束")
    return 1
    #df.to_excel('1.xlsx', sheet_name='Sheet1', index=False, header=True)
def readCsv(infoListSrc=None):
    dataInfos=pd.read_csv(infoListSrc,encoding="utf-8")#('data/Goods_info.csv',encoding="gbk")
    array=[]
    df = pd.DataFrame(dataInfos)
    for index,item in df.iterrows():
        #print(pd.isnull(item["商品编码"]))
        if pd.isnull(item["商品编码"]):
            obj={"订单号":item["订单号"],"商品编码":"编码缺失","名称":item["商品名称"],"数量":item["商品数量"],"价格":item["商品实际成交金额"]}
        else:
            obj={"订单号":item["订单号"],"商品编码":item["商品编码"],"名称":item["商品名称"],"数量":item["商品数量"],"价格":item["商品实际成交金额"]}
        array.append(obj)
    return array
def readrefCsv(refSrc=None):
    dataRefInfos=pd.read_csv(refSrc,encoding="utf-8")#('data/Goods_info.csv',encoding="gbk")
    array=[]
    df = pd.DataFrame(dataRefInfos)
    for index,item in df.iterrows():
        #print(pd.isnull(item["商品编码"]))
        if pd.isnull(item["商品编码"]):
            obj={"订单编号":item["订单编号"],"商品编码":"编码缺失","退款金额":item["退款金额"],"售后状态":item["售后状态"],"退款申请时间":item["申请时间"]}
        else:
            obj={"订单编号":item["订单编号"],"商品编码":item["商品编码"],"退款金额":item["退款金额"],"售后状态":item["售后状态"],"退款申请时间":item["申请时间"]}
        if obj["售后状态"]=="退款成功":
            array.append(obj)
    return array
def readFreightCsv(freightSrc=None):
    dataFreightInofs=pd.read_csv(freightSrc,encoding="utf-8")
    array=[]
    df=pd.DataFrame(dataFreightInofs)
    for index,item in df.iterrows():
        obj={"订单号":item["订单号"],"订单配送方式":item["订单配送方式"],"运费":item["运费"]}
        # if obj["订单配送方式"] == "快递":
        #     #print(obj)
        #     array.append(obj)
        array.append(obj)
    return array
if __name__ == "__main__":
    #readFreightCsv("F:\\账单测试数据\\GoodsOrderNew.csv")
    #main()
    main(str(sys.argv[1]),str(sys.argv[2]),str(sys.argv[3]),str(sys.argv[4]))#str(sys.argv[0]),str(sys.argv[1])