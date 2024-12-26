local mssql = { version = "1.0" }

-- Create connection
function mssql.createConnection(connectionString)
    local connection = {}
    connection.context = create_mssql_connection(connectionString)

    function connection:open()
        self.context:Open()
    end

    function connection:close()
        self.context:Close()
    end

    function connection:isOpen()
        return tostring(self.context.State) == "Open: 1"
    end

    function addParameters(command, params)
        if params ~= nil then
            if type(params) ~= "table" then
                error("Second argument must be a table")
            end
            for key,value in pairs(params) do
                command.Parameters:AddWithValue('@'.. key, value)
            end
        end
    end

    function connection:executeScalar(query, params)
        local command = self.context:CreateCommand()
        command.CommandText = query
        addParameters(command, params)
        result = command:ExecuteScalar()
        return result
    end

    function connection:executeReader(query, params)
        local command = self.context:CreateCommand()
        command.CommandText = query
        addParameters(command, params)
        local reader = {}
        reader.dataReader = command:ExecuteReader()
        function reader:read()
            return self.dataReader:Read()
        end
        function reader:getData()
            return self.dataReader
        end
        return reader
    end

    function connection:execute(query, params)
        local command = self.context:CreateCommand()
        command.CommandText = query
        addParameters(command, params)
        return command:ExecuteNonQuery()
    end

    return connection
end

return mssql