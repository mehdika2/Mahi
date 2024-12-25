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

    function connection:executeScalar(query)
        local command = self.context:CreateCommand()
        command.CommandText = query
        result = command:ExecuteScalar()
        return result
    end

    function connection:executeReader(query)
        local command = self.context:CreateCommand()
        command.CommandText = query
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

    function connection:execute(query)
        local command = self.context:CreateCommand()
        command.CommandText = query
        return command:ExecuteNonQuery()
    end

    return connection
end

return mssql