object:subClass("buff2001")
-- 当前测试每两秒打印一次
buff2001.tickInterval = 2
buff2001.lifeTime = 10

buff2001.timer = 0
buff2001.lifetimer = 0

local function buffTrigger()
    print("buff2001 Trigger")
end

function buff2001:BuffOnOccur(buff)
	buff2001.currentStack = buff.CurrentStack
	print(buff.CurrentStack)
end

function buff2001:BuffRemoveOccur()
	print("buff2001 Remove")
end

function buff2001:BuffOnTick(time)
	CS.Tools.Battle.BattleManager.Instance:Test()
	
	print("buff2001 OnTick")
    buff2001.timer = buff2001.timer + time
    buff2001.lifetimer = buff2001.lifetimer + time
    if buff2001.timer >= buff2001.tickInterval then
        buff2001.timer = 0
        buffTrigger()
    end
	return false
end

function buff2001:BuffOnHit(damageInfo, target)
	print("buff2001 OnHit")
end

function buff2001:BuffOnBeHurt(damageInfo, attacker)
	print("buff2001 OnBeHurt")
end

function buff2001:BuffOnKill(damageInfo, target)
	print("buff2001 OnKill")
end

function buff2001:BuffCanRemove(time, force)
	print("buff2001 OnCanRemove")
    if buff2001.lifetimer >= buff2001.lifeTime then
		print("move")
        return true
    end
	return false
end

return buff2001

