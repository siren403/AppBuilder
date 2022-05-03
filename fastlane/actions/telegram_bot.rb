
module Fastlane
    module Actions
        module SharedValues
            TELEGRAM_BOT_CUSTOM_VALUE = :TELEGRAM_BOT_CUSTOM_VALUE
        end

        class TelegramBotAction < Action
            def self.run(params)

                # fastlane will take care of reading in the parameter and fetching the environment variable:
                # UI.message "Parameter API Token: #{params[:api_token]}"

                # sh "shellcommand ./path"

                # Actions.lane_context[SharedValues::TELEGRAM_BOT_CUSTOM_VALUE] = "my_val"

                token = params[:token]
                chat_id = params[:chat_id]
                text = params[:text]
                parse_mode = params[:parse_mode]

                UI.message token
                UI.message chat_id
                UI.message parse_mode

                Telegram::Bot::Client.run(token) do |bot|
                    bot.api.send_message(
                        chat_id: chat_id,
                        text: text,
                        parse_mode: parse_mode
                    )
                end
            end

            #####################################################
            # @!group Documentation
            #####################################################

            def self.description
                "A short description with <= 80 characters of what this action does"
            end

            def self.details
                # Optional:
                # this is your chance to provide a more detailed description of this action
                "You can use this action to do cool things..."
            end

            def self.available_options
                # Define all options your action supports.

                # Below a few examples
                [
                    FastlaneCore::ConfigItem.new(key: :token,
                                                 env_name: "TELEGRAM_TOKEN", # The name of the environment variable
                                                 description: "API Token for TelegramBotAction", # a short description of this parameter
                                                 optional: false,
                                                 type: String),
                    FastlaneCore::ConfigItem.new(key: :chat_id,
                                                 env_name: "TELEGRAM_CHAT_ID",
                                                 description: "chat_id", optional: false,
                                                 optional: false,
                                                 type: String), # the default value if the user didn't provide one
                    FastlaneCore::ConfigItem.new(key: :text,
                                                 env_name: "TELEGRAM_CHAT_ID",
                                                 description: "text",
                                                 optional: false,
                                                 type: String),
                    FastlaneCore::ConfigItem.new(key: :parse_mode,
                                                 env_name: "TELEGRAM_PARSE_MODE",
                                                 description: "parse_mode",
                                                 optional: true,
                                                 type: String,
                                                 default_value: "MarkdownV2"),
                ]
            end

            def self.output
                # Define the shared values you are going to provide
                # Example
                [
                    ['TELEGRAM_BOT_CUSTOM_VALUE', 'A description of what this value contains']
                ]
            end

            def self.return_value
                # If your method provides a return value, you can describe here what it does
            end

            def self.authors
                # So no one will ever forget your contribution to fastlane :) You are awesome btw!
                ["Your GitHub/Twitter Name"]
            end

            def self.is_supported?(platform)
                # you can do things like
                #
                #  true
                #
                #  platform == :ios
                #
                #  [:ios, :mac].include?(platform)
                #

                # platform == :ios
                [:ios, :android].include?(platform)
            end
        end
    end
end
