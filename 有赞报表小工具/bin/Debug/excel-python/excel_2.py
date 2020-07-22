# coding=utf-8
import  pandas  as pd
from openpyxl import Workbook
import time
import sys
import re,sys
class excel_data:
    def __init__(self,data,infoList):
        self.data=data
#入账详情
class excel_data_in:
    def __init__(self,data,infoList):
        self.data=data
        self.infoList=infoList
#获取入账
def getTableAll(infoSrc='C:\\Users\\tiantao\\Desktop\\报表\\账单明细.xlsx',infoListSrc='C:\\Users\\tiantao\\Desktop\\报表\\Goods.csv',refSrc='C:\\Users\\tiantao\\Desktop\\报表\\Refund.csv'):
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
    ref_array=readrefCsv1(refSrc)
    print("退款明细表共"+str(len(ref_array))+"条数据")
    print("[结束]拿取退款明细表")

    df = pd.DataFrame(data)
    print("[开始]清洗账单明细中数据")
    for index,item in df.iterrows():
        #print(index)
        #print(item['类型'])
        if item['类型']=="订单入账":
            infolist=[]
            for info in array2:
                if item["关联单号"]==info["订单号"]:
                    #print(item["关联单号"])
                    info["退款金额"]=""
                    for refInfo in ref_array:
                        if info["订单号"]==refInfo["订单编号"] and info["商品编码"]==refInfo["商品编码"]:
                            #print("退款金额:")
                            #print(refInfo["退款金额"])
                            info["退款金额"]=refInfo["退款金额"]
                            break#此处怕有同一订单，同一编码退多次的情况出现
                    infolist.append(info)
            obj=excel_data_in(item,infolist)#[{"名称":"蛇草水","价格":"10.05","数量":"1"},{"名称":"矿泉水","价格":"8.05","数量":"1"}]
            array.append(obj)
    print("明细表共"+str(len(array))+"条数据")
    return array
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
def readrefCsv1(refSrc=None):
    dataRefInfos=pd.read_csv(refSrc,encoding="utf-8")#('data/Goods_info.csv',encoding="gbk")
    array=[]
    df = pd.DataFrame(dataRefInfos)
    for index,item in df.iterrows():
        #print(pd.isnull(item["商品编码"]))
        if pd.isnull(item["商品编码"]):
            obj={"订单编号":item["订单编号"],"商品编码":"编码缺失","退款金额":item["退款金额"],"售后状态":item["售后状态"]}
        else:
            obj={"订单编号":item["订单编号"],"商品编码":item["商品编码"],"退款金额":item["退款金额"],"售后状态":item["售后状态"]}
        if obj["售后状态"]=="退款成功":
            array.append(obj)
    return array

def main(infoListSrc='data/2/goods.csv',refSrc='data/2/ref.csv',infoSrc='C:\\Users\\tiantao\\Desktop\\报表\\账单明细.xlsx'):
    print(infoListSrc,refSrc,infoSrc)
    arrayIn=getTableAll(infoSrc,infoListSrc,refSrc)
    print("[开始]拿取产品明细")
    data =pd.read_csv(infoListSrc,encoding="utf-8")#('data/账单明细.xlsx')
    print("明细表共"+str(len(data))+"条数据")
    print("[结束]拿取产品明细")
    array=[]
    print("[开始]拿取退款明细表")
    ref_array=readrefCsv(refSrc)
    print("退款明细表共"+str(len(ref_array))+"条数据")
    print("[结束]拿取退款明细表")
    dff=None
    df = pd.DataFrame(data)
    df['退款金额']=""
    df['退款申请时间']=""
    df['入账金额']=""
    df['入账日期']=""
    print("[开始]添加退款数据")
    for index,item in df.iterrows():
        if pd.isnull(item["商品编码"]):
            df.at[index,'商品编码']="编码缺失"
        # if item['订单商品状态']=="已发货" or item['订单商品状态']=="交易完成":
        for refInfo in ref_array:
            if item["订单号"]==refInfo["订单编号"] and item["商品编码"]==refInfo["商品编码"]:
                    # print("退款金额:")
                    # print(refInfo["退款金额"])
                    # item['退款金额']=refInfo["退款金额"]
                df.at[index,'退款金额']=refInfo["退款金额"]
                df.at[index,'退款申请时间']=refInfo["退款申请时间"]
                    #print(item)
                break#此处怕有同一订单，同一编码退多次的情况出现
        for obj in arrayIn:
            if item["订单号"]==obj.data["关联单号"]:
                for i in range(0,len(obj.infoList)):
                    if item["商品编码"]==obj.infoList[i]["商品编码"]:
                        df.at[index,'入账金额']=obj.infoList[i]["价格"]
                        df.at[index,'入账日期']=obj.data["入账时间"]
                        break                    
    print("[结束]添加退款数据")
    time_stamp = time.strftime('%Y-%m-%d-%H-%M-%S',time.localtime(time.time()))
    addr =str(time_stamp)+".xlsx"#"openpyxl.xlsx"
    wb = Workbook()
    print("创建新excel成功")
    # 激活 worksheet
    df.to_excel(addr, sheet_name='Sheet1', index=False, header=True)

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
if __name__ == "__main__":
    ##main(infoListSrc='C:\\Users\\tiantao\\Desktop\\报表\\Goods.csv',refSrc='C:\\Users\\tiantao\\Desktop\\报表\\Refund.csv',infoSrc='C:\\Users\\tiantao\\Desktop\\报表\\账单明细.xlsx')
    #main('data/2/goods.csv','data/2/ref.csv')
    main(str(sys.argv[1]),str(sys.argv[2]),str(sys.argv[3]))#str(sys.argv[0]),str(sys.argv[1])